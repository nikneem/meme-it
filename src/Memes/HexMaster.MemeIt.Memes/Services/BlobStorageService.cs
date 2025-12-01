using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using HexMaster.MemeIt.Memes.Abstractions.Services;

namespace HexMaster.MemeIt.Memes.Services;

/// <summary>
/// Service implementation for Azure Blob Storage operations.
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private const string ContainerName = "meme-templates";
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    }

    /// <inheritdoc />
    public async Task<(string BlobUrl, string SasToken, DateTimeOffset ExpiresAt)> GenerateUploadSasTokenAsync(
        string blobName,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        // Get or create the container
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
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
            BlobContainerName = ContainerName,
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
}
