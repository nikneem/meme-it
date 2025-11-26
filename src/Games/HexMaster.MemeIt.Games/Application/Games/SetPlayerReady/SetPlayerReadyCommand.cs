using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games.SetPlayerReady;

/// <summary>
/// Command issued when a player toggles their ready state in the lobby.
/// </summary>
/// <param name="PlayerId">Identifier of the player changing their ready state.</param>
/// <param name="GameCode">Code of the game.</param>
/// <param name="IsReady">Whether the player is ready to start.</param>
public sealed record SetPlayerReadyCommand(Guid PlayerId, string GameCode, bool IsReady)
    : ICommand<SetPlayerReadyResult>;
