namespace HexMaster.MemeIt.IntegrationEvents.Events;

/// <summary>
/// Published when a round ends with the current scoreboard.
/// </summary>
public sealed record RoundEndedEvent(
    string GameCode,
    int RoundNumber,
    int TotalRounds,
    IReadOnlyCollection<ScoreboardEntryDto> Scoreboard);

/// <summary>
/// Represents a player's score on the scoreboard.
/// </summary>
public sealed record ScoreboardEntryDto(
    Guid PlayerId,
    string PlayerName,
    int TotalScore);
