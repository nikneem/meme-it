namespace HexMaster.MemeIt.IntegrationEvents.Events;

public sealed record ScorePhaseStartedEvent(
    string GameCode,
    int RoundNumber,
    Guid MemeId,
    Guid PlayerId,
    Guid MemeTemplateId,
    IReadOnlyCollection<MemeTextEntryDto> TextEntries);

public sealed record MemeTextEntryDto(Guid TextFieldId, string Value);
