using System;

namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response returned after submitting a meme.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="PlayerId">The player ID.</param>
/// <param name="RoundNumber">The round number.</param>
/// <param name="MemeTemplateId">The meme template ID.</param>
/// <param name="TextEntryCount">The number of text entries submitted.</param>
public sealed record SubmitMemeResponse(
    string GameCode,
    Guid PlayerId,
    int RoundNumber,
    Guid MemeTemplateId,
    int TextEntryCount);
