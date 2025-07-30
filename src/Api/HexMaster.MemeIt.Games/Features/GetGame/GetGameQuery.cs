using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.GetGame;

public record GetGameQuery(string GameId, string? PlayerId = null) : IQuery;