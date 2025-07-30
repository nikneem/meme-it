using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.ValueObjects;
using Localizr.Core.Abstractions.Cqrs;
using Orleans;

namespace HexMaster.MemeIt.Games.Features.JoinGame;

public sealed class JoinGameCommandHandler(IGrainFactory grainFactory) : ICommandHandler<JoinGameCommand, GameDetailsResponse>
{
    public async ValueTask<GameDetailsResponse> HandleAsync(JoinGameCommand command,
        CancellationToken cancellationToken)
    {
        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        var newGameState = await gameGrain.JoinGame(command);
        
        // Find the player ID for the joining player by name
        var joiningPlayerId = newGameState.Players.FirstOrDefault(p => p.Name == command.PlayerName).Id ?? string.Empty;
        
        return GameDetailsResponse.FromGameState(newGameState, joiningPlayerId);
    }
}