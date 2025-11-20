namespace HexMaster.MemeIt.IntegrationEvents.Events;

public sealed record PlayerJoinedEvent(System.Guid PlayerId, string DisplayName, string GameCode);
