using Orleans;

namespace HexMaster.MemeIt.Games.ValueObjects;

[GenerateSerializer]
public class GameState
{
    [Id(0)] public required string GameCode { get; set; }
    [Id(1)] public required string Status { get; set; } = GameStatus.Uninitialized.Id;
    [Id(2)] public required List<(string Id, string Name)> Players { get; set; }
    [Id(3)] public string? Password { get; set; }
    [Id(4)] public string? LeaderId { get; set; } // PlayerId of the leader
    [Id(5)] public GameSettings Settings { get; set; } = new(); // Game settings (max players, rounds, category)
}
