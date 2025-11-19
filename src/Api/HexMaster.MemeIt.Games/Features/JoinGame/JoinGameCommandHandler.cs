using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Games.Services;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.ValueObjects;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.JoinGame;

public sealed class JoinGameCommandHandler(IGameService gameService) : ICommandHandler<JoinGameCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(JoinGameCommand command,
        CancellationToken cancellationToken)
    {
        var newGameState = await gameService.JoinGameAsync(command, cancellationToken);
        
        // Find the player ID for the joining player by name
        var joiningPlayerId = newGameState.Players.FirstOrDefault(p => p.Name == command.PlayerName).Id ?? string.Empty;
        
        return GameDetailsResponse.FromGameState(newGameState, joiningPlayerId);
    }
}