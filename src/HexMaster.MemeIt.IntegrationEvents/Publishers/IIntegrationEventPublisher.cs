namespace HexMaster.MemeIt.IntegrationEvents.Publishers;

public interface IIntegrationEventPublisher
{
    Task PublishPlayerStateChangedAsync(Events.PlayerStateChangedEvent @event, CancellationToken cancellationToken = default);
    Task PublishPlayerRemovedAsync(Events.PlayerRemovedEvent @event, CancellationToken cancellationToken = default);
    Task PublishPlayerJoinedAsync(Events.PlayerJoinedEvent @event, CancellationToken cancellationToken = default);
}
