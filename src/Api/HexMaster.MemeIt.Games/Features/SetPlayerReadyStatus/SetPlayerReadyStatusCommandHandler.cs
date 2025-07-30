using HexMaster.MemeIt.Games.Abstractions.Grains;
using Orleans;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.SetPlayerReadyStatus;

public class SetPlayerReadyStatusCommandHandler(IGrainFactory grainFactory) : ICommandHandler<SetPlayerReadyStatusCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(SetPlayerReadyStatusCommand command, CancellationToken cancellationToken)
    {
        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        var gameState = await gameGrain.SetPlayerReadyStatus(command.PlayerId, command.IsReady);
        return GameDetailsResponse.FromGameState(gameState, command.PlayerId);
    }
}
