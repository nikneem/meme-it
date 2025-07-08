using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.JoinGame;

public record JoinGameCommand(string GameCode, string PlayerName, string? Password) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
}
