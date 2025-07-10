using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.StartGame;

public record StartGameCommand(string PlayerId, string GameCode) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
}
