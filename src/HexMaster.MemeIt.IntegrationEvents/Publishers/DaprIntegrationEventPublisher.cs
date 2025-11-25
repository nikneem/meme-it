using Dapr.Client;
using HexMaster.MemeIt.IntegrationEvents.Events;

namespace HexMaster.MemeIt.IntegrationEvents.Publishers;

public sealed class DaprIntegrationEventPublisher(DaprClient daprClient) : IIntegrationEventPublisher
{


    public Task PublishPlayerStateChangedAsync(PlayerStateChangedEvent @event, CancellationToken cancellationToken = default)
    {
        // Publish using Dapr client
        return daprClient.PublishEventAsync(DaprConstants.PubSubName, DaprConstants.Topics.PlayerStateChanged, @event, cancellationToken: cancellationToken);
    }

    public Task PublishPlayerRemovedAsync(PlayerRemovedEvent @event, CancellationToken cancellationToken = default)
    {
        return daprClient.PublishEventAsync(DaprConstants.PubSubName, DaprConstants.Topics.PlayerRemoved, @event, cancellationToken: cancellationToken);
    }

    public Task PublishPlayerJoinedAsync(PlayerJoinedEvent @event, CancellationToken cancellationToken = default)
    {
        return daprClient.PublishEventAsync(DaprConstants.PubSubName, DaprConstants.Topics.PlayerJoined, @event, cancellationToken: cancellationToken);
    }
}
