using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games.StartGame;

/// <summary>
/// Command issued when a game admin starts the game.
/// </summary>
/// <param name="GameCode">The game code to start.</param>
/// <param name="AdminPlayerId">The player ID of the admin starting the game.</param>
public sealed record StartGameCommand(string GameCode, Guid AdminPlayerId)
    : ICommand<StartGameResult>;
