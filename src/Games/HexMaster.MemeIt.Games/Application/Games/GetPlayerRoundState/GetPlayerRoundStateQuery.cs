using HexMaster.MemeIt.Games.Abstractions.Application.Queries;

namespace HexMaster.MemeIt.Games.Application.Games.GetPlayerRoundState;

/// <summary>
/// Query to get the current player's round state.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="PlayerId">The player ID.</param>
public sealed record GetPlayerRoundStateQuery(string GameCode, Guid PlayerId) : IQuery;
