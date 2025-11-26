using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games.JoinGame;

/// <summary>
/// Command issued when a player joins an existing game.
/// </summary>
/// <param name="PlayerId">Identifier of the player joining the game.</param>
/// <param name="PlayerName">Display name of the player.</param>
/// <param name="GameCode">Code of the game to join.</param>
/// <param name="Password">Optional password if the game is protected.</param>
public sealed record JoinGameCommand(Guid PlayerId, string PlayerName, string GameCode, string? Password)
    : ICommand<JoinGameResult>;
