using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.ValueObjects;
using Localizr.Core.Abstractions.Cqrs;
using Orleans;

namespace HexMaster.MemeIt.Games.Features.CreateGame;

public class CreateGameCommandHandler(IGrainFactory grainFactory) : ICommandHandler<CreateGameCommand, CreateGameResponse>
{
    public async ValueTask<CreateGameResponse> HandleAsync(CreateGameCommand command, CancellationToken cancellationToken)
    {
        var gameCode = Randomizer.GenerateGameCode();

        var initialGameState = new CreateGameState
        {
            Password = command.Password,
            PlayerName = command.PlayerName,
        };
        
        var gameGrain = grainFactory.GetGrain<IGameGrain>(gameCode);
        var gameState = await gameGrain.CreateGame(initialGameState);
        return new CreateGameResponse(gameState.GameCode);
    }
}