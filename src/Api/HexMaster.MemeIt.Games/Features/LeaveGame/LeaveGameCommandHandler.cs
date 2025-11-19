using HexMaster.MemeIt.Games.Services;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Games.Features.LeaveGame;

using Localizr.Core.Abstractions.Cqrs;

public class LeaveGameCommandHandler(IGameService gameService) : ICommandHandler<LeaveGameCommand, LeaveGameResponse>
{
    public async ValueTask<LeaveGameResponse> HandleAsync(LeaveGameCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await gameService.LeaveGameAsync(command.GameCode, command.PlayerId, cancellationToken);
            return new LeaveGameResponse(Guid.Parse(command.PlayerId));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Game has been deleted"))
        {
            // Game was deleted because no players remain, still return success for the leaving player
            return new LeaveGameResponse(Guid.Parse(command.PlayerId));
        }
    }
}
