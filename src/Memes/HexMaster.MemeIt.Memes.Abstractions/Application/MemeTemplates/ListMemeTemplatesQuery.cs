using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Domains;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;

/// <summary>
/// Query to list all meme templates.
/// </summary>
public record ListMemeTemplatesQuery : IQuery;

/// <summary>
/// Result of listing all meme templates.
/// </summary>
public record ListMemeTemplatesResult(IReadOnlyList<MemeTemplateDto> Templates);
