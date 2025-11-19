using HexMaster.MemeIt.Games.Services;

namespace HexMaster.MemeIt.Games.Features.UpdateSettings;

using Localizr.Core.Abstractions.Cqrs;

public class UpdateSettingsCommandHandler(IGameService gameService)
    : ICommandHandler<UpdateSettingsCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(UpdateSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var gameState = await gameService.UpdateSettingsAsync(command.GameCode, command.PlayerId, command.Settings, cancellationToken);
        return GameDetailsResponse.FromGameState(gameState, command.PlayerId);
    }
}