namespace HexMaster.MemeIt.IntegrationEvents.Events;

public sealed record PlayerRemovedEvent(System.Guid PlayerId, string DisplayName);
