namespace HexMaster.MemeIt.Games.Application.Games.SubmitMeme;

public sealed record SubmitMemeResult(
    string GameCode,
    Guid PlayerId,
    int RoundNumber,
    Guid MemeTemplateId,
    int TextEntryCount);
