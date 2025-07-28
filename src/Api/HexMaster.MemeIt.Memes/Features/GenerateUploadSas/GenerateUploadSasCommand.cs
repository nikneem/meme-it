using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.GenerateUploadSas;

public class GenerateUploadSasCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}
