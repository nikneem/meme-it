namespace HexMaster.MemeIt.IntegrationEvents.Events;

/// <summary>
/// Published when a new round starts.
/// </summary>
public sealed record RoundStartedEvent(string GameCode, int RoundNumber, int DurationInSeconds);
