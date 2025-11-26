using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Services;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates.GenerateUploadSasToken;

/// <summary>
/// Handler for generating SAS tokens for blob storage upload.
/// </summary>
public class GenerateUploadSasTokenQueryHandler(IBlobStorageService blobStorageService)
    : IQueryHandler<GenerateUploadSasTokenQuery, GenerateUploadSasTokenResult>
{

    public async Task<GenerateUploadSasTokenResult> HandleAsync(
        GenerateUploadSasTokenQuery query,
        CancellationToken cancellationToken = default)
    {
        // Generate a unique blob name with timestamp to avoid collisions
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var blobName = $"{timestamp}-{Guid.NewGuid()}.png";

        // Generate SAS token using the blob storage service
        var (blobUrl, sasToken, expiresAt) = await blobStorageService.GenerateUploadSasTokenAsync(
            blobName,
            expirationMinutes: 60,
            cancellationToken);

        // Get container URL (remove the blob name from the full URL)
        var containerUrl = blobUrl.Substring(0, blobUrl.LastIndexOf('/'));

        var result = new GenerateUploadSasTokenResult(
            blobUrl,
            sasToken,
            containerUrl,
            expiresAt
        );

        return result;
    }
}
