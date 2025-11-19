using System;

namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response returned after successfully joining a game.
/// </summary>
/// <param name="GameCode">Code of the game that was joined.</param>
/// <param name="PlayerId">Identifier of the player who joined.</param>
/// <param name="State">Current state of the game.</param>
public sealed record JoinGameResponse(string GameCode, Guid PlayerId, string State);
