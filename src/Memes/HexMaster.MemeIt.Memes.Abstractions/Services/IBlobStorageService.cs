namespace HexMaster.MemeIt.Memes.Abstractions.Services;

/// <summary>
/// Service interface for Azure Blob Storage operations.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Generates a SAS token for uploading a blob to the meme-templates container.
    /// </summary>
    /// <param name="blobName">The name of the blob (file) to upload.</param>
    /// <param name="expirationMinutes">How long the SAS token should be valid (default 60 minutes).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the blob URL and SAS token.</returns>
    Task<(string BlobUrl, string SasToken, DateTimeOffset ExpiresAt)> GenerateUploadSasTokenAsync(
        string blobName,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default);
}
