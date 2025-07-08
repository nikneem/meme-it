using HexMaster.MemeIt.Core;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.CreateGame;

public record CreateGameCommand(string PlayerName, string? Password) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();

}