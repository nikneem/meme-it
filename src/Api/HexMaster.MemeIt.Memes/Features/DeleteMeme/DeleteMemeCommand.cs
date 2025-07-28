using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.DeleteMeme;

public record DeleteMemeCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public required string Id { get; init; }
}
