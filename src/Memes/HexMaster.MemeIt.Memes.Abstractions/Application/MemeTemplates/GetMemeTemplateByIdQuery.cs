using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Domains;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;

/// <summary>
/// Query to get a meme template by ID.
/// </summary>
public record GetMemeTemplateByIdQuery(Guid Id) : IQuery;

/// <summary>
/// Result of getting a meme template by ID.
/// </summary>
public record GetMemeTemplateByIdResult(MemeTemplateDto? Template);
