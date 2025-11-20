namespace HexMaster.MemeIt.IntegrationEvents.Events;

public sealed record PlayerStateChangedEvent(System.Guid PlayerId, string DisplayName, bool IsReady, string GameCode);
