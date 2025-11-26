using HexMaster.MemeIt.Games.Abstractions.ValueObjects;

namespace HexMaster.MemeIt.Games.Application.Games.JoinGame;

/// <summary>
/// Result returned after a player successfully joins a game.
/// </summary>
/// <param name="GameCode">Code of the game that was joined.</param>
/// <param name="PlayerId">Identifier of the player who joined.</param>
/// <param name="State">Current state of the game.</param>
public sealed record JoinGameResult(string GameCode, Guid PlayerId, GameState State);
