namespace HexMaster.MemeIt.Memes.DataTransferObjects;

public record GenerateUploadSasRequest
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}
