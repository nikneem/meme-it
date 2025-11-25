namespace HexMaster.MemeIt.IntegrationEvents.Events;

public sealed record ScorePhaseStartedEvent(
    string GameCode,
    int RoundNumber,
    Guid MemeId,
    Guid PlayerId,
    Guid MemeTemplateId,
    IReadOnlyCollection<MemeTextEntryDto> TextEntries,
    int RatingDurationSeconds = 30);

public sealed record MemeTextEntryDto(Guid TextFieldId, string Value);
