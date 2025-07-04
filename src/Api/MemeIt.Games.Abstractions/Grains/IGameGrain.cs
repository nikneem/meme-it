using MemeIt.Games.Abstractions.Models;
using Orleans;

namespace MemeIt.Games.Abstractions.Grains;

public interface IGameGrain : IGrainWithStringKey
{
    Task<GameState> GetGameStateAsync();
    Task<bool> JoinGameAsync(string playerId, string playerName, string? password = null);
    Task<bool> LeaveGameAsync(string playerId);
    Task<bool> UpdateGameOptionsAsync(string playerId, GameOptions options);
    Task<bool> StartGameAsync(string playerId);
    Task<bool> SubmitMemeTextAsync(string playerId, Dictionary<string, string> textEntries);
    Task<bool> SubmitScoreAsync(string playerId, string targetPlayerId, int score);
    Task<List<string>> GetPlayersAsync();
    Task<Dictionary<string, int>> GetScoresAsync();
    Task<RoundState?> GetCurrentRoundAsync();
    Task<bool> CancelGameAsync(string playerId);
    Task NotifyPlayerActivity(string playerId);
}
