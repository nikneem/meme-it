using System;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Result returned after creating a new game.
/// </summary>
/// <param name="GameCode">Lobby code players use to join.</param>
/// <param name="AdminPlayerId">Identifier of the admin player.</param>
/// <param name="State">Lifecycle state of the game after creation.</param>
/// <param name="CreatedAt">Timestamp for when the game was created.</param>
public sealed record CreateGameResult(string GameCode, Guid AdminPlayerId, GameState State, DateTimeOffset CreatedAt);
