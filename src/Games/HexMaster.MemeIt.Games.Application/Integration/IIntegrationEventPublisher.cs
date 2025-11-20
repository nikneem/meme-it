using System.Threading;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Games.Application.Integration;

public interface IIntegrationEventPublisher
{
    Task PublishPlayerStateChangedAsync(HexMaster.MemeIt.IntegrationEvents.Events.PlayerStateChangedEvent @event, CancellationToken cancellationToken = default);
    Task PublishPlayerJoinedAsync(HexMaster.MemeIt.IntegrationEvents.Events.PlayerJoinedEvent @event, CancellationToken cancellationToken = default);
}
