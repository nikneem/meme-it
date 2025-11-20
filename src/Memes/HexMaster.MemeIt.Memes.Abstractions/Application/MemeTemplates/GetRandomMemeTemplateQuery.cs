using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Domains;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;

/// <summary>
/// Query to get a random meme template.
/// </summary>
public record GetRandomMemeTemplateQuery : IQuery;

/// <summary>
/// Result of getting a random meme template.
/// </summary>
public record GetRandomMemeTemplateResult(MemeTemplateDto? Template);
