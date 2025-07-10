using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
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

    public async Task<GameState> CreateGame(CreateGameCommand createGameState)
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
            Password = createGameState.Password,
            LeaderId = initialPlayerState.Id,
            Settings = new Dictionary<string, string>()
        };
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public async Task<GameState> JoinGame(JoinGameCommand playerState)
    {
        if (state.State.Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Cannot join a game that is not in waiting status.");
        }
        if (!string.IsNullOrWhiteSpace(state.State.Password) && !Equals(state.State.Password, playerState.Password))
        {
            throw new ArgumentException("The provided password does not match the game's password.");
        }
        if (state.State.Players.Any(p => p.Name == playerState.PlayerName))
        {
            throw new InvalidOperationException("A player with this name already joined.");
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

    public async Task<GameState> LeaveGame(string playerId)
    {
        if (state.State.Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Cannot leave a game that is not in waiting status.");
        }
        var player = state.State.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == default)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }
        state.State.Players.Remove(player);
        // If leader leaves, assign new leader if any players remain
        if (state.State.LeaderId == playerId)
        {
            state.State.LeaderId = state.State.Players.FirstOrDefault().Id;
        }
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public async Task<GameState> UpdateSettings(string playerId, Dictionary<string, string> settings)
    {
        if (state.State.LeaderId != playerId)
        {
            throw new UnauthorizedAccessException("Only the game leader can update settings.");
        }
        if (state.State.Settings == null)
        {
            state.State.Settings = new Dictionary<string, string>();
        }
        foreach (var kvp in settings)
        {
            state.State.Settings[kvp.Key] = kvp.Value;
        }
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public async Task<GameState> StartGame(string playerId)
    {
        if (state.State.LeaderId != playerId)
        {
            throw new UnauthorizedAccessException("Only the game leader can start the game.");
        }
        if (state.State.Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Game can only be started from waiting status.");
        }
        state.State.Status = GameStatus.Active.Id;
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }
}