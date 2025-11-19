using HexMaster.MemeIt.Games.Services;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Games.Features.StartGame;

using Localizr.Core.Abstractions.Cqrs;

public class StartGameCommandHandler(IGameService gameService)
    : ICommandHandler<StartGameCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(StartGameCommand command, CancellationToken cancellationToken)
    {
        var gameState = await gameService.StartGameAsync(command.GameCode, command.PlayerId, cancellationToken);
        return GameDetailsResponse.FromGameState(gameState, command.PlayerId);
    }

}
