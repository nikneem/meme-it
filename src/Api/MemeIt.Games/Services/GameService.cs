using Orleans;
using Microsoft.Extensions.Logging;
using MemeIt.Games.Utilities;
using MemeIt.Games.Abstractions.Services;
using MemeIt.Games.Abstractions.Grains;
using MemeIt.Games.Abstractions.Models;

namespace MemeIt.Games.Services;

public class GameService : IGameService
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<GameService> _logger;

    public GameService(IGrainFactory grainFactory, ILogger<GameService> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task<(string GameId, string GameCode)> CreateGameAsync(string playerId, string playerName, string gameName, GameOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(playerId))
            throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
        if (string.IsNullOrWhiteSpace(playerName))
            throw new ArgumentException("Player name cannot be null or empty", nameof(playerName));
        if (string.IsNullOrWhiteSpace(gameName))
            throw new ArgumentException("Game name cannot be null or empty", nameof(gameName));

        var gameId = Guid.NewGuid().ToString();
        var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);

        // Initialize game state by joining as the first player (which makes them game master)
        var success = await gameGrain.JoinGameAsync(playerId, playerName);
        
        if (!success)
        {
            throw new InvalidOperationException("Failed to create game");
        }

        // Update game options if provided
        if (options != null)
        {
            await gameGrain.UpdateGameOptionsAsync(playerId, options);
        }

        // Get the game state to retrieve the generated game code
        var gameState = await gameGrain.GetGameStateAsync();
        gameState.Name = gameName;

        _logger.LogInformation("Created game {GameId} with code {GameCode} and name '{GameName}' by player {PlayerId}", 
            gameId, gameState.GameCode, gameName, playerId);

        return (gameId, gameState.GameCode);
    }

    public async Task<GameState?> GetGameAsync(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
            throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));

        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            return await gameGrain.GetGameStateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game {GameId}", gameId);
            return null;
        }
    }

    public async Task<GameState?> GetGameByCodeAsync(string gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
            throw new ArgumentException("Game code cannot be null or empty", nameof(gameCode));

        try
        {
            var normalizedCode = GameCodeGenerator.NormalizeGameCode(gameCode);
            if (normalizedCode == null)
            {
                _logger.LogWarning("Invalid game code format: {GameCode}", gameCode);
                return null;
            }

            var gameRegistry = _grainFactory.GetGrain<IGameRegistryGrain>(0);
            var gameId = await gameRegistry.GetGameIdByCodeAsync(normalizedCode);
            
            if (gameId == null)
            {
                _logger.LogWarning("Game not found for code: {GameCode}", normalizedCode);
                return null;
            }

            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            return await gameGrain.GetGameStateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game by code {GameCode}", gameCode);
            return null;
        }
    }

    public Task<List<GameState>> GetAvailableGamesAsync()
    {
        // In a real implementation, this would query a game registry or database
        // For now, return empty list as we don't have a centralized game registry
        _logger.LogDebug("GetAvailableGamesAsync called - returning empty list (not implemented)");
        return Task.FromResult(new List<GameState>());
    }

    public async Task<bool> JoinGameAsync(string gameId, string playerId, string playerName, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(gameId))
            throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));
        if (string.IsNullOrWhiteSpace(playerId))
            throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
        if (string.IsNullOrWhiteSpace(playerName))
            throw new ArgumentException("Player name cannot be null or empty", nameof(playerName));

        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var result = await gameGrain.JoinGameAsync(playerId, playerName, password);
            
            if (result)
            {
                _logger.LogInformation("Player {PlayerId} ({PlayerName}) joined game {GameId}", 
                    playerId, playerName, gameId);
            }
            else
            {
                _logger.LogWarning("Player {PlayerId} failed to join game {GameId}", playerId, gameId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game {GameId} for player {PlayerId}", gameId, playerId);
            return false;
        }
    }

    public async Task<bool> JoinGameByCodeAsync(string gameCode, string playerId, string playerName, string? password = null)
    {
        try
        {
            var normalizedCode = GameCodeGenerator.NormalizeGameCode(gameCode);
            if (normalizedCode == null)
            {
                _logger.LogWarning("Invalid game code format: {GameCode}", gameCode);
                return false;
            }

            var gameRegistry = _grainFactory.GetGrain<IGameRegistryGrain>(0);
            var gameId = await gameRegistry.GetGameIdByCodeAsync(normalizedCode);
            
            if (gameId == null)
            {
                _logger.LogWarning("Game not found for code: {GameCode}", normalizedCode);
                return false;
            }

            return await JoinGameAsync(gameId, playerId, playerName, password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game by code {GameCode} for player {PlayerId}", gameCode, playerId);
            return false;
        }
    }

    public async Task<bool> LeaveGameAsync(string gameId, string playerId)
    {
        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var result = await gameGrain.LeaveGameAsync(playerId);
            
            if (result)
            {
                _logger.LogInformation("Player {PlayerId} left game {GameId}", playerId, gameId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving game {GameId} for player {PlayerId}", gameId, playerId);
            return false;
        }
    }

    public async Task<bool> StartGameAsync(string gameId, string playerId)
    {
        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var result = await gameGrain.StartGameAsync(playerId);
            
            if (result)
            {
                _logger.LogInformation("Game {GameId} started by player {PlayerId}", gameId, playerId);
            }
            else
            {
                _logger.LogWarning("Failed to start game {GameId} by player {PlayerId}", gameId, playerId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting game {GameId} by player {PlayerId}", gameId, playerId);
            return false;
        }
    }

    public async Task<bool> SubmitMemeTextAsync(string gameId, string playerId, Dictionary<string, string> textEntries)
    {
        if (string.IsNullOrWhiteSpace(gameId))
            throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));
        if (string.IsNullOrWhiteSpace(playerId))
            throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
        if (textEntries == null)
            throw new ArgumentException("Text entries cannot be null", nameof(textEntries));

        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var result = await gameGrain.SubmitMemeTextAsync(playerId, textEntries);
            
            if (result)
            {
                _logger.LogInformation("Player {PlayerId} submitted meme text for game {GameId}", playerId, gameId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting meme text for game {GameId} by player {PlayerId}", gameId, playerId);
            return false;
        }
    }

    public async Task<bool> SubmitScoreAsync(string gameId, string playerId, string targetPlayerId, int score)
    {
        if (string.IsNullOrWhiteSpace(gameId))
            throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));
        if (string.IsNullOrWhiteSpace(playerId))
            throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
        if (string.IsNullOrWhiteSpace(targetPlayerId))
            throw new ArgumentException("Target player ID cannot be null or empty", nameof(targetPlayerId));
        if (score < 1 || score > 5)
            throw new ArgumentException("Score must be between 1 and 5", nameof(score));

        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            var result = await gameGrain.SubmitScoreAsync(playerId, targetPlayerId, score);
            
            if (result)
            {
                _logger.LogInformation("Player {PlayerId} scored player {TargetPlayerId} with {Score} in game {GameId}", 
                    playerId, targetPlayerId, score, gameId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting score for game {GameId} by player {PlayerId}", gameId, playerId);
            return false;
        }
    }

    public async Task<Dictionary<string, int>> GetScoresAsync(string gameId)
    {
        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            return await gameGrain.GetScoresAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scores for game {GameId}", gameId);
            return new Dictionary<string, int>();
        }
    }

    public async Task<RoundState?> GetCurrentRoundAsync(string gameId)
    {
        try
        {
            var gameGrain = _grainFactory.GetGrain<IGameGrain>(gameId);
            return await gameGrain.GetCurrentRoundAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current round for game {GameId}", gameId);
            return null;
        }
    }
}
