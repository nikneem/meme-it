using Orleans;

namespace MemeIt.Games.Abstractions.Grains;

public interface IGameRegistryGrain : IGrainWithIntegerKey
{
    Task<string?> RegisterGameAsync(string gameId, string gameCode);
    Task<string?> GetGameIdByCodeAsync(string gameCode);
    Task<bool> UnregisterGameAsync(string gameCode);
    Task<bool> IsGameCodeAvailableAsync(string gameCode);
    Task<List<string>> GetActiveGameCodesAsync();
}
