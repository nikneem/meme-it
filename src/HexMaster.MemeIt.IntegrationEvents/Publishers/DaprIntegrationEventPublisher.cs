using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using HexMaster.MemeIt.IntegrationEvents.Events;

namespace HexMaster.MemeIt.IntegrationEvents.Publishers;

public sealed class DaprIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly Dapr.Client.DaprClient _daprClient;
    private readonly string _pubsubName = "chatservice-pubsub";

    public DaprIntegrationEventPublisher(Dapr.Client.DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public Task PublishPlayerStateChangedAsync(PlayerStateChangedEvent @event, CancellationToken cancellationToken = default)
    {
        // Publish using Dapr client
        return _daprClient.PublishEventAsync(_pubsubName, "playerstatechanged", @event, cancellationToken: cancellationToken);
    }

    public Task PublishPlayerRemovedAsync(PlayerRemovedEvent @event, CancellationToken cancellationToken = default)
    {
        return _daprClient.PublishEventAsync(_pubsubName, "playerremoved", @event, cancellationToken: cancellationToken);
    }
}
