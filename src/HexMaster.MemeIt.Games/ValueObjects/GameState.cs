using Orleans;

namespace HexMaster.MemeIt.Games.ValueObjects;

[GenerateSerializer]
public class GameState
{
    [Id(0)] public required string GameCode { get; init; }
    [Id(1)] public required string Status { get; init; } = GameStatus.Uninitialized.Id;
    [Id(2)] public required List<(string Id, string Name)> Players { get; init; }
    [Id(3)] public string? Password { get; init; }
}
