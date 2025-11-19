using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Games.Services;
using HexMaster.MemeIt.Games.ValueObjects;
using Localizr.Core.Abstractions.Cqrs;
using System.Linq;

namespace HexMaster.MemeIt.Games.Features.GetGame;

public class GetGameQueryHandler(IGameService gameService) : IQueryHandler<GetGameQuery, OperationResult<GameDetailsResponse>>
{
    public async ValueTask<OperationResult<GameDetailsResponse>> HandleAsync(GetGameQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var currentGameState = await gameService.GetCurrentAsync(query.GameId, cancellationToken);
            
            if (Equals(currentGameState.Status, GameStatus.Uninitialized.Id))
            {
                return new OperationResult<GameDetailsResponse>(false, null);
            }

            var playerId = query.PlayerId ?? string.Empty;
            
            // If a playerId is provided, validate that the player exists in the game
            if (!string.IsNullOrEmpty(query.PlayerId))
            {
                var playerExists = currentGameState.Players.Any(p => p.Id == query.PlayerId);
                if (!playerExists)
                {
                    return new OperationResult<GameDetailsResponse>(false, null);
                }
            }

            return new OperationResult<GameDetailsResponse>(true, GameDetailsResponse.FromGameState(currentGameState, playerId));
        }
        catch (InvalidOperationException)
        {
            // Game not found
            return new OperationResult<GameDetailsResponse>(false, null);
        }
    }
}