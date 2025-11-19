using System;

namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response returned after setting player ready state.
/// </summary>
/// <param name="PlayerId">Identifier of the player.</param>
/// <param name="IsReady">New ready state.</param>
/// <param name="AllPlayersReady">Whether all players are now ready.</param>
public sealed record SetPlayerReadyResponse(Guid PlayerId, bool IsReady, bool AllPlayersReady);
