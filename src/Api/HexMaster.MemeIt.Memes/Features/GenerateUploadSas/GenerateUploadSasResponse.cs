namespace HexMaster.MemeIt.Memes.Features.GenerateUploadSas;

public record GenerateUploadSasResponse
{
    public string SasUri { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}
