using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Games.Services;
using HexMaster.MemeIt.Games.ValueObjects;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.CreateGame;

public class CreateGameCommandHandler(IGameService gameService) : ICommandHandler<CreateGameCommand, CreateGameResponse>
{
    public async ValueTask<CreateGameResponse> HandleAsync(CreateGameCommand command, CancellationToken cancellationToken)
    {
        var gameState = await gameService.CreateGameAsync(command, cancellationToken);
        
        // Get the player ID for the creator (should be the first player)
        var creatorPlayerId = gameState.Players.FirstOrDefault().Id ?? string.Empty;
        
        return CreateGameResponse.FromGameState(gameState, creatorPlayerId);
    }
}