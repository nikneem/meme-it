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
        var joinGameState = new JoinGameState
        {
            PlayerName = command.PlayerName,
            Password = command.Password
        };


        var gameGrain = grainFactory.GetGrain<IGameGrain>(command.GameCode);
        var newGameState = await gameGrain.JoinGame(joinGameState);
        return GameDetailsResponse.FromGameState(newGameState);
    }
}