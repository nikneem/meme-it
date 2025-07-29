using HexMaster.MemeIt.Memes.DataTransferObjects;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.CreateMeme;

public record CreateMemeCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string SourceImage { get; init; }
    public required int SourceWidth { get; init; }
    public required int SourceHeight { get; init; }
    public required MemeTextArea[] TextAreas { get; init; }
}
