using System;

namespace HexMaster.MemeIt.Games.Application.Games;

public sealed record SubmitMemeResult(
    string GameCode,
    Guid PlayerId,
    int RoundNumber,
    Guid MemeTemplateId,
    int TextEntryCount);
