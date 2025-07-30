using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.KickPlayer;

public record KickPlayerCommand(string HostPlayerId, string TargetPlayerId, string GameCode) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
}
