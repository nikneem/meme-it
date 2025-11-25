using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;

namespace HexMaster.MemeIt.Memes.Api.Requests;

/// <summary>
/// HTTP request payload for updating a meme template.
/// </summary>
public record UpdateMemeTemplateRequest(
    string Title,
    string ImageUrl,
    int Width,
    int Height,
    IReadOnlyList<TextAreaDefinitionDto> TextAreas
);
