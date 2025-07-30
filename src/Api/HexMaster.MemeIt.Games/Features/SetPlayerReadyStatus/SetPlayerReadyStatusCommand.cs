using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.SetPlayerReadyStatus;

public record SetPlayerReadyStatusCommand(string PlayerId, string GameCode, bool IsReady) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
}
