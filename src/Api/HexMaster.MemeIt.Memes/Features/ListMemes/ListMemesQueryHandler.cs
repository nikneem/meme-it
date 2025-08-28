using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using HexMaster.MemeIt.Memes.Services;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.ListMemes;

public class ListMemesQueryHandler : IQueryHandler<ListMemesQuery, IEnumerable<MemeTemplateListResponse>>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobUrlService _blobUrlService;

    public ListMemesQueryHandler(IMemeTemplateRepository repository, IBlobUrlService blobUrlService)
    {
        _repository = repository;
        _blobUrlService = blobUrlService;
    }

    public async ValueTask<IEnumerable<MemeTemplateListResponse>> HandleAsync(ListMemesQuery query, CancellationToken cancellationToken)
    {
        var memeTemplates = await _repository.GetAllAsync(cancellationToken);
        
        return memeTemplates.Select(mt => new MemeTemplateListResponse(
            mt.Id,
            mt.Name,
            mt.Description,
            _blobUrlService.GetMemeImageUrl(mt.SourceImageUrl),
            mt.CreatedAt)).ToList();
    }
}
