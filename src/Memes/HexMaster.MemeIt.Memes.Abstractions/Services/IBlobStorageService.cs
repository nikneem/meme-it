namespace HexMaster.MemeIt.Memes.Abstractions.Services;

/// <summary>
/// Service interface for Azure Blob Storage operations.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Generates a SAS token for uploading a blob to a specified container.
    /// </summary>
    /// <param name="containerName">The name of the container.</param>
    /// <param name="blobName">The name of the blob (file) to upload.</param>
    /// <param name="expirationMinutes">How long the SAS token should be valid (default 60 minutes).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the blob URL and SAS token.</returns>
    Task<(string BlobUrl, string SasToken, DateTimeOffset ExpiresAt)> GenerateUploadSasTokenAsync(
        string containerName,
        string blobName,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a blob from one container to another.
    /// </summary>
    /// <param name="sourceBlobName">The name of the source blob.</param>
    /// <param name="sourceContainerName">The source container name.</param>
    /// <param name="destinationContainerName">The destination container name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The destination blob URL.</returns>
    Task<string> MoveBlobAsync(
        string sourceBlobName,
        string sourceContainerName,
        string destinationContainerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a blob from a container.
    /// </summary>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="containerName">The container name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the blob was deleted, false if it didn't exist.</returns>
    Task<bool> DeleteBlobAsync(
        string blobName,
        string containerName,
        CancellationToken cancellationToken = default);
}
