using Orleans;

namespace HexMaster.MemeIt.Games.ValueObjects;

[GenerateSerializer]
public record JoinGameState
{
    [Id(0)] public required string PlayerName { get; init; }
    [Id(1)] public string? Password { get; init; }

}