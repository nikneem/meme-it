using HexMaster.MemeIt.Games.Abstractions.Grains;
using Orleans;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Games.Features.LeaveGame;

using Localizr.Core.Abstractions.Cqrs;

public class LeaveGameCommandHandler(IGrainFactory grainFactory) : ICommandHandler<LeaveGameCommand, LeaveGameResponse>
{
    public async ValueTask<LeaveGameResponse> HandleAsync(LeaveGameCommand command, CancellationToken cancellationToken)
    {
        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        await gameGrain.LeaveGame(command.PlayerId);
        return new LeaveGameResponse(Guid.Parse(command.PlayerId));
    }
}
