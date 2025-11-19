using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.JoinGame;

public record JoinGameCommand : ICommand
{
    public required string GameCode { get; init; }
    public required string PlayerName { get; init; }
    public string? Password { get; init; }
    public Guid CommandId { get; } = Guid.NewGuid();
}