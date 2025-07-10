using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.LeaveGame;

public record LeaveGameCommand(string PlayerId, string GameCode) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
}
