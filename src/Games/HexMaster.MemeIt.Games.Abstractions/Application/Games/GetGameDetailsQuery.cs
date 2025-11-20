using System;
using HexMaster.MemeIt.Games.Abstractions.Application.Queries;

namespace HexMaster.MemeIt.Games.Abstractions.Application.Games;

/// <summary>
/// Query to retrieve game details by game code.
/// </summary>
/// <param name="GameCode">The unique game code.</param>
/// <param name="RequestingPlayerId">The ID of the player requesting the game details.</param>
public sealed record GetGameDetailsQuery(string GameCode, Guid RequestingPlayerId) : IQuery;
