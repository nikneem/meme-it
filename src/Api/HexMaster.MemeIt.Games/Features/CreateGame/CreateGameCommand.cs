using HexMaster.MemeIt.Core;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.CreateGame;

public record CreateGameCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public required string PlayerName { get; init; }
    public required string GameCode { get; init; }
    public string? Password { get; init; }
}