using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;

/// <summary>
/// Query to generate a SAS token for uploading meme template images.
/// </summary>
public record GenerateUploadSasTokenQuery : IQuery;

/// <summary>
/// Result containing the SAS token and upload URL.
/// </summary>
public record GenerateUploadSasTokenResult(
    string BlobUrl,
    string SasToken,
    string ContainerUrl,
    DateTimeOffset ExpiresAt
);
