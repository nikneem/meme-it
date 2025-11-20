using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;

/// <summary>
/// Command to delete a meme template.
/// </summary>
public record DeleteMemeTemplateCommand(Guid Id) : ICommand<DeleteMemeTemplateResult>;

/// <summary>
/// Result of deleting a meme template.
/// </summary>
public record DeleteMemeTemplateResult(bool Success);
