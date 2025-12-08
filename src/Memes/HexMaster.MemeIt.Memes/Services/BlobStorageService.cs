using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using HexMaster.MemeIt.Memes.Abstractions.Services;

namespace HexMaster.MemeIt.Memes.Services;

/// <summary>
/// Service implementation for Azure Blob Storage operations.
/// </summary>
public class BlobStorageService(BlobServiceClient blobServiceClient) : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));

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
        var startsOn = DateTimeOffset.UtcNow.AddMinutes(-5); // Start 5 minutes ago to account for clock skew
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b", // 'b' for blob
            StartsOn = startsOn,
            ExpiresOn = expiresAt
        };

        // Grant write permissions
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        string sasToken;
        var blobUrl = blobClient.Uri.ToString();

        // Check if we can generate SAS with shared key (local development with Azurite)
        if (blobClient.CanGenerateSasUri)
        {
            // Use shared key credential (Azurite/local development)
            sasToken = blobClient.GenerateSasUri(sasBuilder).Query.TrimStart('?');
        }
        else
        {
            // Use user delegation key (Managed Identity in Azure)
            var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(
                startsOn,
                expiresAt,
                cancellationToken);

            // Generate the SAS token using user delegation key
            var sasQueryParameters = sasBuilder.ToSasQueryParameters(userDelegationKey.Value, _blobServiceClient.AccountName);
            sasToken = sasQueryParameters.ToString();
        }

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

    /// <inheritdoc />
    public async Task<bool> DeleteBlobAsync(
        string blobName,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }
}
