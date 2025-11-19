namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Result returned after setting player ready state.
/// </summary>
/// <param name="PlayerId">Identifier of the player.</param>
/// <param name="IsReady">New ready state.</param>
/// <param name="AllPlayersReady">Whether all players are now ready.</param>
public sealed record SetPlayerReadyResult(Guid PlayerId, bool IsReady, bool AllPlayersReady);
