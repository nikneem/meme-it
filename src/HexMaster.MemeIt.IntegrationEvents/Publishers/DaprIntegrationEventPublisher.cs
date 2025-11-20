using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using HexMaster.MemeIt.IntegrationEvents.Events;

namespace HexMaster.MemeIt.IntegrationEvents.Publishers;

public sealed class DaprIntegrationEventPublisher(DaprClient daprClient) : IIntegrationEventPublisher
{
    private readonly string _pubsubName = "chatservice-pubsub";


    public Task PublishPlayerStateChangedAsync(PlayerStateChangedEvent @event, CancellationToken cancellationToken = default)
    {
        // Publish using Dapr client
        return daprClient.PublishEventAsync(_pubsubName, "playerstatechanged", @event, cancellationToken: cancellationToken);
    }

    public Task PublishPlayerRemovedAsync(PlayerRemovedEvent @event, CancellationToken cancellationToken = default)
    {
        return daprClient.PublishEventAsync(_pubsubName, "playerremoved", @event, cancellationToken: cancellationToken);
    }

    public Task PublishPlayerJoinedAsync(PlayerJoinedEvent @event, CancellationToken cancellationToken = default)
    {
        return daprClient.PublishEventAsync(_pubsubName, "playerjoined", @event, cancellationToken: cancellationToken);
    }
}
