using HexMaster.MemeIt.Games.Abstractions.Grains;
using Orleans;

namespace HexMaster.MemeIt.Games.Features.UpdateSettings;

using Localizr.Core.Abstractions.Cqrs;

public class UpdateSettingsCommandHandler(IGrainFactory grainFactory)
    : ICommandHandler<UpdateSettingsCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(UpdateSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        var gameState = await gameGrain.UpdateSettings(command.PlayerId, command.Settings);
        return GameDetailsResponse.FromGameState(gameState);
    }
}