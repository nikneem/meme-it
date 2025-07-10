using Orleans;

namespace HexMaster.MemeIt.Games.ValueObjects;

[GenerateSerializer]
public class GameSettings
{
    [Id(0)] public int MaxPlayers { get; set; } = 10;
    [Id(1)] public int NumberOfRounds { get; set; } = 5;
    [Id(2)] public string Category { get; set; } = "All";
}
