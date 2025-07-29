using HexMaster.MemeIt.Memes.DataTransferObjects;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.UpdateMeme;

public record UpdateMemeCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required MemeTextArea[] TextAreas { get; init; }
}
