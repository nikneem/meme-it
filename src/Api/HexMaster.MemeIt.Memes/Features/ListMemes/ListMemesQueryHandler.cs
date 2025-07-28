using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.ListMemes;

public class ListMemesQueryHandler : IQueryHandler<ListMemesQuery, IEnumerable<MemeTemplateListResponse>>
{
    private readonly IMemeTemplateRepository _repository;

    public ListMemesQueryHandler(IMemeTemplateRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<IEnumerable<MemeTemplateListResponse>> HandleAsync(ListMemesQuery query, CancellationToken cancellationToken)
    {
        var memeTemplates = await _repository.GetAllAsync(cancellationToken);
        
        return memeTemplates.Select(mt => new MemeTemplateListResponse(
            mt.Id,
            mt.Name,
            mt.Description,
            mt.SourceImageUrl,
            mt.CreatedAt)).ToList();
    }
}
