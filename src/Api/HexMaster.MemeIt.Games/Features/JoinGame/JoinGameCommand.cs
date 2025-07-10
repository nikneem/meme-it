using Localizr.Core.Abstractions.Cqrs;
using Orleans;

namespace HexMaster.MemeIt.Games.Features.JoinGame;


[GenerateSerializer]
public record JoinGameCommand : ICommand
{

    [Id(0)] public required string GameCode { get; init; }
    [Id(1)] public required string PlayerName { get; init; }
    [Id(2)] public string? Password { get; init; }
    [Id(3)] public Guid CommandId { get; } = Guid.NewGuid();
}