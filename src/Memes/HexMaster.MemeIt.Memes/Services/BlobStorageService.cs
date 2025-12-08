using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using HexMaster.MemeIt.Memes.Abstractions.Services;

namespace HexMaster.MemeIt.Memes.Services;

/// <summary>
/// Service implementation for Azure Blob Storage operations.
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    }

    /// <inheritdoc />
    public async Task<(string BlobUrl, string SasToken, DateTimeOffset ExpiresAt)> GenerateUploadSasTokenAsync(
        string containerName,
        string blobName,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        // Get or create the container
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // Get the blob client
        var blobClient = containerClient.GetBlobClient(blobName);

        // Set expiration time
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

        // Get a user delegation key for the Blob service (required for managed identity)
        var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            expiresAt,
            cancellationToken);

        // Generate SAS token for upload (write permissions) using user delegation
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b", // 'b' for blob
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Start 5 minutes ago to account for clock skew
            ExpiresOn = expiresAt
        };

        // Grant write permissions
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        // Generate the SAS token using user delegation key
        var sasQueryParameters = sasBuilder.ToSasQueryParameters(userDelegationKey.Value, _blobServiceClient.AccountName);
        var sasToken = sasQueryParameters.ToString();
        var blobUrl = blobClient.Uri.ToString();

        return (blobUrl, sasToken, expiresAt);
    }

    /// <inheritdoc />
    public async Task<string> MoveBlobAsync(
        string sourceBlobName,
        string sourceContainerName,
        string destinationContainerName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceBlobName);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceContainerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationContainerName);

        // Get source and destination container clients
        var sourceContainerClient = _blobServiceClient.GetBlobContainerClient(sourceContainerName);
        var destinationContainerClient = _blobServiceClient.GetBlobContainerClient(destinationContainerName);

        // Ensure destination container exists
        await destinationContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // Get blob clients
        var sourceBlobClient = sourceContainerClient.GetBlobClient(sourceBlobName);
        var destinationBlobClient = destinationContainerClient.GetBlobClient(sourceBlobName);

        // Check if source blob exists
        if (!await sourceBlobClient.ExistsAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Source blob '{sourceBlobName}' does not exist in container '{sourceContainerName}'");
        }

        // Copy blob to destination
        await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri, cancellationToken: cancellationToken);

        // Wait for copy to complete
        var properties = await destinationBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        while (properties.Value.CopyStatus == Azure.Storage.Blobs.Models.CopyStatus.Pending)
        {
            await Task.Delay(100, cancellationToken);
            properties = await destinationBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        }

        if (properties.Value.CopyStatus != Azure.Storage.Blobs.Models.CopyStatus.Success)
        {
            throw new InvalidOperationException($"Failed to copy blob '{sourceBlobName}' from '{sourceContainerName}' to '{destinationContainerName}'");
        }

        // Delete source blob after successful copy
        await sourceBlobClient.DeleteAsync(cancellationToken: cancellationToken);

        return destinationBlobClient.Uri.ToString();
    }
}
