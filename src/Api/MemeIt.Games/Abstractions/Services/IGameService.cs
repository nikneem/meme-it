using MemeIt.Games.Abstractions.Models;

namespace MemeIt.Games.Abstractions.Services;

public interface IGameService
{
    Task<(string GameId, string GameCode)> CreateGameAsync(string playerId, string playerName, string gameName, GameOptions? options = null);
    Task<GameState?> GetGameAsync(string gameId);
    Task<GameState?> GetGameByCodeAsync(string gameCode);
    Task<List<GameState>> GetAvailableGamesAsync();
    Task<bool> JoinGameAsync(string gameId, string playerId, string playerName, string? password = null);
    Task<bool> JoinGameByCodeAsync(string gameCode, string playerId, string playerName, string? password = null);
    Task<bool> LeaveGameAsync(string gameId, string playerId);
    Task<bool> StartGameAsync(string gameId, string playerId);
    Task<bool> SubmitMemeTextAsync(string gameId, string playerId, Dictionary<string, string> textEntries);
    Task<bool> SubmitScoreAsync(string gameId, string playerId, string targetPlayerId, int score);
    Task<Dictionary<string, int>> GetScoresAsync(string gameId);
    Task<RoundState?> GetCurrentRoundAsync(string gameId);
}
