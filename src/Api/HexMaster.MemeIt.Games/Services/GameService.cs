using HexMaster.MemeIt.Games.Abstractions;
using HexMaster.MemeIt.Games.Domain;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Services;

public interface IGameService
{
    Task<GameState> GetCurrentAsync(string gameCode, CancellationToken cancellationToken = default);
    Task<GameState> CreateGameAsync(CreateGameCommand createGameCommand, CancellationToken cancellationToken = default);
    Task<GameState> JoinGameAsync(JoinGameCommand joinGameCommand, CancellationToken cancellationToken = default);
    Task<GameState> LeaveGameAsync(string gameCode, string playerId, CancellationToken cancellationToken = default);
    Task<GameState> KickPlayerAsync(string gameCode, string hostPlayerId, string targetPlayerId, CancellationToken cancellationToken = default);
    Task<GameState> UpdateSettingsAsync(string gameCode, string playerId, GameSettings settings, CancellationToken cancellationToken = default);
    Task<GameState> StartGameAsync(string gameCode, string playerId, CancellationToken cancellationToken = default);
    Task<GameState> SetPlayerReadyStatusAsync(string gameCode, string playerId, bool isReady, CancellationToken cancellationToken = default);
    Task<PlayerMemeAssignment?> GetPlayerMemeAssignmentAsync(string gameCode, string playerId, CancellationToken cancellationToken = default);
    Task<GameState> AssignMemeToPlayerAsync(string gameCode, string playerId, string memeTemplateId, string memeTemplateName, string memeTemplateImageUrl, CancellationToken cancellationToken = default);
}

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GameState> GetCurrentAsync(string gameCode, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetAsync(gameCode, cancellationToken);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{gameCode}' not found.");
        }

        return game.ToGameState();
    }

    public async Task<GameState> CreateGameAsync(CreateGameCommand createGameCommand, CancellationToken cancellationToken = default)
    {
        // Check if game already exists
        var existingGame = await _gameRepository.GetAsync(createGameCommand.GameCode, cancellationToken);
        if (existingGame != null)
        {
            throw new InvalidOperationException($"Game with code '{createGameCommand.GameCode}' already exists.");
        }

        var game = Game.Create(createGameCommand.GameCode, createGameCommand.PlayerName, createGameCommand.Password);
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return game.ToGameState();
    }

    public async Task<GameState> JoinGameAsync(JoinGameCommand joinGameCommand, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(joinGameCommand.GameCode, cancellationToken);
        
        game.JoinPlayer(joinGameCommand.PlayerName, joinGameCommand.Password);
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return game.ToGameState();
    }

    public async Task<GameState> LeaveGameAsync(string gameCode, string playerId, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameCode, cancellationToken);
        
        game.RemovePlayer(playerId);
        
        // If no players left, delete the game
        if (game.Players.Count == 0)
        {
            await _gameRepository.DeleteAsync(gameCode, cancellationToken);
            // Return empty game state or throw exception as appropriate
            throw new InvalidOperationException("Game has been deleted as no players remain.");
        }
        
        await _gameRepository.SaveAsync(game, cancellationToken);
        return game.ToGameState();
    }

    public async Task<GameState> KickPlayerAsync(string gameCode, string hostPlayerId, string targetPlayerId, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameCode, cancellationToken);
        
        game.KickPlayer(hostPlayerId, targetPlayerId);
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return game.ToGameState();
    }

    public async Task<GameState> UpdateSettingsAsync(string gameCode, string playerId, GameSettings settings, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameCode, cancellationToken);
        
        game.UpdateSettings(playerId, settings);
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return game.ToGameState();
    }

    public async Task<GameState> StartGameAsync(string gameCode, string playerId, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameCode, cancellationToken);
        
        game.StartGame(playerId);
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return game.ToGameState();
    }

    public async Task<GameState> SetPlayerReadyStatusAsync(string gameCode, string playerId, bool isReady, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameCode, cancellationToken);
        
        game.SetPlayerReadyStatus(playerId, isReady);
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return game.ToGameState();
    }

    public async Task<PlayerMemeAssignment?> GetPlayerMemeAssignmentAsync(string gameCode, string playerId, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameCode, cancellationToken);
        return game.GetPlayerMemeAssignment(playerId);
    }

    public async Task<GameState> AssignMemeToPlayerAsync(string gameCode, string playerId, string memeTemplateId, string memeTemplateName, string memeTemplateImageUrl, CancellationToken cancellationToken = default)
    {
        var game = await GetGameAsync(gameCode, cancellationToken);
        
        game.AssignMemeToPlayer(playerId, memeTemplateId, memeTemplateName, memeTemplateImageUrl);
        await _gameRepository.SaveAsync(game, cancellationToken);
        
        return game.ToGameState();
    }

    private async Task<Game> GetGameAsync(string gameCode, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetAsync(gameCode, cancellationToken);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{gameCode}' not found.");
        }
        return game;
    }
}
