using MemeIt.Games.Abstractions.Models;
using Orleans;

namespace MemeIt.Games.Abstractions.Grains;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task<PlayerState> GetPlayerStateAsync();
    Task<bool> JoinGameAsync(string gameId, string playerName);
    Task<bool> LeaveGameAsync();
    Task UpdateActivityAsync();
    Task SetStatusAsync(PlayerStatus status);
    Task<string?> GetCurrentGameAsync();
    Task<bool> IsInGameAsync();
}
