using System;

namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response containing the player's current round state.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="PlayerId">The player ID.</param>
/// <param name="RoundNumber">The current round number.</param>
/// <param name="RoundStartedAt">When the round started.</param>
/// <param name="SelectedMemeTemplateId">The ID of the meme template selected by the player (null if not yet selected).</param>
public sealed record GetPlayerRoundStateResponse(
    string GameCode,
    Guid PlayerId,
    int RoundNumber,
    DateTimeOffset RoundStartedAt,
    Guid? SelectedMemeTemplateId);
