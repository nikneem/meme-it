namespace HexMaster.MemeIt.Memes.DataTransferObjects;

public record MemeTemplateResponse(
    string Id,
    string Name,
    string? Description,
    string SourceImageUrl,
    int SourceWidth,
    int SourceHeight,
    MemeTextArea[] TextAreas,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
