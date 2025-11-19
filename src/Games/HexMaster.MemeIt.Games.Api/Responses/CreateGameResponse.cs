using System;

namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response returned after creating a game.
/// </summary>
/// <param name="GameCode">Lobby code assigned to the game.</param>
/// <param name="AdminPlayerId">Identifier of the admin player.</param>
/// <param name="CreatedAt">Creation timestamp.</param>
/// <param name="State">Lifecycle state string.</param>
public sealed record CreateGameResponse(string GameCode, Guid AdminPlayerId, DateTimeOffset CreatedAt, string State);
