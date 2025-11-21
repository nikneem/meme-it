namespace HexMaster.MemeIt.IntegrationEvents.Events;

public sealed record GameStartedEvent(string GameCode, int RoundNumber);
