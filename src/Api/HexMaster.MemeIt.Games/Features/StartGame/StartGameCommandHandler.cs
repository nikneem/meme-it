using HexMaster.MemeIt.Games.Abstractions.Grains;
using Orleans;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Games.Features.StartGame;

using Localizr.Core.Abstractions.Cqrs;

public class StartGameCommandHandler(IGrainFactory grainFactory)
    : ICommandHandler<StartGameCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(StartGameCommand command, CancellationToken cancellationToken)
    {
        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        var gameState = await gameGrain.StartGame(command.PlayerId);
        return GameDetailsResponse.FromGameState(gameState);
    }

}
