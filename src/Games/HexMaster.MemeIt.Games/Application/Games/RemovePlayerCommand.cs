using System;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Command issued when a game admin removes a player from the game.
/// </summary>
/// <param name="AdminPlayerId">Identifier of the player making the request (must be admin).</param>
/// <param name="GameCode">Code of the game to remove the player from.</param>
/// <param name="PlayerIdToRemove">Identifier of the player to be removed.</param>
public sealed record RemovePlayerCommand(Guid AdminPlayerId, string GameCode, Guid PlayerIdToRemove)
    : ICommand<RemovePlayerResult>;
