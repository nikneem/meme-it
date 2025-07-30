using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.ValueObjects;
using Localizr.Core.Abstractions.Cqrs;
using Orleans;

namespace HexMaster.MemeIt.Games.Features.GetGame;

public class GetGameQueryHandler(IGrainFactory grainFactory) : IQueryHandler<GetGameQuery, OperationResult<GameDetailsResponse>>
{
    public async ValueTask<OperationResult<GameDetailsResponse>> HandleAsync(GetGameQuery query, CancellationToken cancellationToken)
    {
        var gameGrain = grainFactory.GetGrain<IGameGrain>(query.GameId);
        var currentGameState = await gameGrain.GetCurrent();
        if (Equals(currentGameState.Status, GameStatus.Uninitialized.Id))
        {
            return new OperationResult<GameDetailsResponse>(false, null);
        }

        var playerId = query.PlayerId ?? string.Empty;
        return new OperationResult<GameDetailsResponse>(true, GameDetailsResponse.FromGameState(currentGameState, playerId));
    }
}