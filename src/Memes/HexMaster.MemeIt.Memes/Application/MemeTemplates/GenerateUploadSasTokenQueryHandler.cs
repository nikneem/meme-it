using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates;

/// <summary>
/// Handler for generating SAS tokens for blob storage upload.
/// </summary>
public class GenerateUploadSasTokenQueryHandler : IQueryHandler<GenerateUploadSasTokenQuery, GenerateUploadSasTokenResult>
{
    public Task<GenerateUploadSasTokenResult> HandleAsync(
        GenerateUploadSasTokenQuery query,
        CancellationToken cancellationToken = default)
    {
        // Generate a unique blob name
        var blobName = $"meme-templates/{Guid.NewGuid()}.png";

        // Get storage configuration (hardcoded for now, should come from configuration)
        var storageAccountName = "memeitstoragedev";
        var containerName = "meme-templates";

        // Generate SAS token (expires in 1 hour)
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // In a real implementation, this would use Azure.Storage.Blobs to generate actual SAS tokens
        var containerUrl = $"https://{storageAccountName}.blob.core.windows.net/{containerName}";
        var blobUrl = $"{containerUrl}/{blobName}";

        // TODO: Generate actual SAS token using Azure.Storage.Blobs SDK
        // Example: Use BlobServiceClient and GenerateSasUri
        var sasToken = GeneratePlaceholderSasToken(expiresAt);

        var result = new GenerateUploadSasTokenResult(
            blobUrl,
            sasToken,
            containerUrl,
            expiresAt
        );

        return Task.FromResult(result);
    }

    private static string GeneratePlaceholderSasToken(DateTimeOffset expiresAt)
    {
        // This is a placeholder. In production, use Azure.Storage.Blobs to generate real SAS tokens
        var expiryString = expiresAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
        return $"sv=2021-06-08&ss=b&srt=o&sp=w&se={expiryString}&st={DateTimeOffset.UtcNow:yyyy-MM-ddTHH:mm:ssZ}&spr=https&sig=PLACEHOLDER_SIGNATURE";
    }
}
