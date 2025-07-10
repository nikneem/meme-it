using HexMaster.MemeIt.Core;
using Localizr.Core.Abstractions.Cqrs;
using Orleans;

namespace HexMaster.MemeIt.Games.Features.CreateGame;

[GenerateSerializer]
public record CreateGameCommand : ICommand
{
    [Id(0)] public Guid CommandId { get; } = Guid.NewGuid();
    [Id(1)] public required string PlayerName { get; init; }
    [Id(2)] public required string GameCode { get; init; }
    [Id(3)] public string? Password { get; init; }
}