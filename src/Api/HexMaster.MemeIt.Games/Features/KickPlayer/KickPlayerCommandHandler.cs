using HexMaster.MemeIt.Games.Services;
using HexMaster.MemeIt.Games.Features;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.KickPlayer;

public class KickPlayerCommandHandler(IGameService gameService) : ICommandHandler<KickPlayerCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(KickPlayerCommand command, CancellationToken cancellationToken)
    {
        var updatedGameState = await gameService.KickPlayerAsync(command.GameCode, command.HostPlayerId, command.TargetPlayerId, cancellationToken);
        
        return GameDetailsResponse.FromGameState(updatedGameState, command.HostPlayerId);
    }
}
