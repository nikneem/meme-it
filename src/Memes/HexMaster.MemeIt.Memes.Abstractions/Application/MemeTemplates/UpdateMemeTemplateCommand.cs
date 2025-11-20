using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;

/// <summary>
/// Command to update an existing meme template.
/// </summary>
public record UpdateMemeTemplateCommand(
    Guid Id,
    string Title,
    string ImageUrl,
    IReadOnlyList<TextAreaDefinitionDto> TextAreas
) : ICommand<UpdateMemeTemplateResult>;

/// <summary>
/// Result of updating a meme template.
/// </summary>
public record UpdateMemeTemplateResult(bool Success);
