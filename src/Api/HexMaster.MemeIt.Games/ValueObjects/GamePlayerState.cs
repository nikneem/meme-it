using Orleans;

namespace HexMaster.MemeIt.Games.ValueObjects;

[GenerateSerializer]
public class GamePlayerState
{
    [Id(0)] public required string Id { get; init; }
    [Id(1)] public required string Name { get; init; }
}
