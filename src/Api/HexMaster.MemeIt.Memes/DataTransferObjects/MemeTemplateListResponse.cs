namespace HexMaster.MemeIt.Memes.DataTransferObjects;

public record MemeTemplateListResponse(
    string Id,
    string Name,
    string? Description,
    string SourceImageUrl,
    DateTimeOffset CreatedAt);
