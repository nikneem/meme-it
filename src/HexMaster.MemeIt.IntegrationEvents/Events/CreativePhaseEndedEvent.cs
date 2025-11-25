namespace HexMaster.MemeIt.IntegrationEvents.Events;

public sealed record CreativePhaseEndedEvent(string GameCode, int RoundNumber);
