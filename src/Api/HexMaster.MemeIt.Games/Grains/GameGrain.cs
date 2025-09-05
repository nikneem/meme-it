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
            Settings = new GameSettings(),
            PlayerReadyStates = new Dictionary<string, bool> { { initialPlayerState.Id, false } },
            PlayerMemeAssignments = new Dictionary<string, PlayerMemeAssignment>(),
            CurrentRound = 1
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
        state.State.PlayerReadyStates[newPlayerState.Id] = false; // New players start as not ready
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
        state.State.PlayerReadyStates.Remove(playerId); // Clean up ready state
        // If leader leaves, assign new leader if any players remain
        if (state.State.LeaderId == playerId)
        {
            state.State.LeaderId = state.State.Players.FirstOrDefault().Id;
        }
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public async Task<GameState> KickPlayer(string hostPlayerId, string targetPlayerId)
    {
        if (state.State.Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Cannot kick players when the game is not in waiting status.");
        }
        
        // Verify that the host is the leader
        if (state.State.LeaderId != hostPlayerId)
        {
            throw new UnauthorizedAccessException("Only the game host can kick players.");
        }
        
        // Cannot kick yourself
        if (hostPlayerId == targetPlayerId)
        {
            throw new InvalidOperationException("Cannot kick yourself from the game.");
        }
        
        var targetPlayer = state.State.Players.FirstOrDefault(p => p.Id == targetPlayerId);
        if (targetPlayer == default)
        {
            throw new InvalidOperationException("Target player not found in this game.");
        }
        
        // Remove the player
        state.State.Players.Remove(targetPlayer);
        state.State.PlayerReadyStates.Remove(targetPlayerId); // Clean up ready state
        
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public async Task<GameState> UpdateSettings(string playerId, GameSettings settings)
    {
        if (state.State.LeaderId != playerId)
        {
            throw new UnauthorizedAccessException("Only the game leader can update settings.");
        }
        if (state.State.Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Settings can only be changed before the game has started.");
        }
        // Enforce limits
        if (settings.MaxPlayers < 2 || settings.MaxPlayers > 25)
            settings.MaxPlayers = Math.Clamp(settings.MaxPlayers, 2, 25);
        if (settings.NumberOfRounds < 1 || settings.NumberOfRounds > 10)
            settings.NumberOfRounds = Math.Clamp(settings.NumberOfRounds, 1, 10);
        if (string.IsNullOrWhiteSpace(settings.Category))
            settings.Category = "All";
        state.State.Settings = settings;
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
        
        // Check that all players are ready
        var allPlayersReady = state.State.Players.All(p => state.State.PlayerReadyStates.GetValueOrDefault(p.Id, false));
        if (!allPlayersReady)
        {
            throw new InvalidOperationException("All players must be ready before starting the game.");
        }
        
        // Require at least 2 players
        if (state.State.Players.Count < 2)
        {
            throw new InvalidOperationException("At least 2 players are required to start the game.");
        }
        
        state.State.Status = GameStatus.Active.Id;
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public async Task<GameState> SetPlayerReadyStatus(string playerId, bool isReady)
    {
        if (state.State.Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Player ready status can only be changed while the game is waiting.");
        }
        
        var player = state.State.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == default)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }

        // Update the player's ready status in the game state
        state.State.PlayerReadyStates[playerId] = isReady;
        
        // Also update the player grain's state
        var playerGrain = grainFactory.GetGrain<IGamePlayerGrain>(playerId);
        await playerGrain.SetReadyStatus(isReady);
        
        await state.WriteStateAsync();
        _gameWatchers.Notify(watcher => watcher.OnGameUpdated(state.State));
        return state.State;
    }

    public Task<PlayerMemeAssignment?> GetPlayerMemeAssignment(string playerId)
    {
        // Validate player exists in the game
        var player = state.State.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == default)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }

        // Check if player has an assignment for the current round
        var key = $"{playerId}_Round{state.State.CurrentRound}";
        var assignment = state.State.PlayerMemeAssignments.GetValueOrDefault(key);
        return Task.FromResult(assignment);
    }

    public async Task<GameState> AssignMemeToPlayer(string playerId, string memeTemplateId, string memeTemplateName, string memeTemplateImageUrl)
    {
        // Validate player exists in the game
        var player = state.State.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == default)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }

        // Only allow this when game is active/in progress
        if (state.State.Status != GameStatus.Active.Id && state.State.Status != "InProgress")
        {
            throw new InvalidOperationException("Memes can only be assigned when the game is active.");
        }

        // Create the assignment key (player + round)
        var key = $"{playerId}_Round{state.State.CurrentRound}";
        
        // Create or update the assignment
        state.State.PlayerMemeAssignments[key] = new PlayerMemeAssignment
        {
            MemeTemplateId = memeTemplateId,
            MemeTemplateName = memeTemplateName,
            MemeTemplateImageUrl = memeTemplateImageUrl,
            CurrentRound = state.State.CurrentRound,
            AssignedAt = DateTime.UtcNow
        };

        await state.WriteStateAsync();
        return state.State;
    }
}