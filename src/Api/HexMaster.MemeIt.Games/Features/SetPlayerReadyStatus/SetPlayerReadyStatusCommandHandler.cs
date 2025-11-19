using HexMaster.MemeIt.Games.Services;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.SetPlayerReadyStatus;

public class SetPlayerReadyStatusCommandHandler(IGameService gameService) : ICommandHandler<SetPlayerReadyStatusCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(SetPlayerReadyStatusCommand command, CancellationToken cancellationToken)
    {
        var gameState = await gameService.SetPlayerReadyStatusAsync(command.GameCode, command.PlayerId, command.IsReady, cancellationToken);
        return GameDetailsResponse.FromGameState(gameState, command.PlayerId);
    }
}
