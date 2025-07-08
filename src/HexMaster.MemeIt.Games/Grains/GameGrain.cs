using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.ValueObjects;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Utilities;

namespace HexMaster.MemeIt.Games.Grains;

public class GameGrain(IGrainFactory grainFactory,
    [PersistentState(stateName: "gameState", storageName: "games")] IPersistentState<GameState> state,
    ILogger<ObserverManager<IGameGainObserver>> gameLogger) : Grain, IGameGrain
{
    private readonly ObserverManager<IGameGainObserver> _gameWatchers = new(TimeSpan.FromMinutes(1), gameLogger);
    public Task<GameState> GetCurrent() => Task.FromResult(state.State);

    public async Task<GameState> CreateGame(CreateGameState createGameState)
    {
        var initialPlayerState = new GamePlayerState
        {
            Id = Guid.NewGuid().ToString(),
            Name = createGameState.PlayerName
        };

        var playerGrain = grainFactory.GetGrain<IGamePlayerGrain>(initialPlayerState.Id);
        await playerGrain.CreatePlayer(initialPlayerState);

        state.State = new GameState
        {
            GameCode = createGameState.GameCode,
            Players = [(initialPlayerState.Id, initialPlayerState.Name)],
            Status = GameStatus.Waiting.Id,
            Password = createGameState.Password
        };
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public async  Task<GameState> JoinGame(JoinGameState playerState)
    {
        if (!string.IsNullOrWhiteSpace(state.State.Password) && !Equals(state.State.Password, playerState.Password))
        {
            throw new ArgumentException("The provided password does not match the game's password.");
        }

        var newPlayerState = new GamePlayerState
        {
            Id = Guid.NewGuid().ToString(),
            Name = playerState.PlayerName,
        };

        var playerGrain = grainFactory.GetGrain<IGamePlayerGrain>(newPlayerState.Id);
        await playerGrain.CreatePlayer(newPlayerState);

        state.State.Players.Add((newPlayerState.Id, newPlayerState.Name));
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }
}