using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;

/// <summary>
/// Command to create a new meme template.
/// </summary>
public record CreateMemeTemplateCommand(
    string Title,
    string ImageUrl,
    IReadOnlyList<TextAreaDefinitionDto> TextAreas
) : ICommand<CreateMemeTemplateResult>;

/// <summary>
/// Result of creating a meme template.
/// </summary>
public record CreateMemeTemplateResult(Guid Id);
