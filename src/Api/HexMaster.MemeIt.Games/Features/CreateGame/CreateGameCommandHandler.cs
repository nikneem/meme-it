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
        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        var gameState = await gameGrain.CreateGame(command);
        
        // Get the player ID for the creator (should be the first player)
        var creatorPlayerId = gameState.Players.FirstOrDefault().Id ?? string.Empty;
        
        return CreateGameResponse.FromGameState(gameState, creatorPlayerId);
    }
}