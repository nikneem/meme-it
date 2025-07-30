using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.Features;
using Localizr.Core.Abstractions.Cqrs;
using Orleans;

namespace HexMaster.MemeIt.Games.Features.KickPlayer;

public class KickPlayerCommandHandler(IGrainFactory grainFactory) : ICommandHandler<KickPlayerCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(KickPlayerCommand command, CancellationToken cancellationToken)
    {
        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        var updatedGameState = await gameGrain.KickPlayer(command.HostPlayerId, command.TargetPlayerId);
        
        return GameDetailsResponse.FromGameState(updatedGameState, command.HostPlayerId);
    }
}
