using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;

namespace HexMaster.MemeIt.Memes.Abstractions.Domains;

/// <summary>
/// DTO representing a meme template.
/// </summary>
public record MemeTemplateDto(
    Guid Id,
    string Title,
    string ImageUrl,
    IReadOnlyList<TextAreaDefinitionDto> TextAreas,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
