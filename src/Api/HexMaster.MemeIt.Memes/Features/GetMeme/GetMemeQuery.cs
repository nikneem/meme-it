using Localizr.Core.Abstractions.Cqrs;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.MemeIt.Memes.Features.GetMeme;

public record GetMemeQuery : IQuery
{
    [FromRoute]
    public required string Id { get; init; }
}
