using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using HexMaster.MemeIt.Memes.Services;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.GetMeme;

public class GetMemeQueryHandler : IQueryHandler<GetMemeQuery, OperationResult<MemeTemplateResponse>>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobUrlService _blobUrlService;

    public GetMemeQueryHandler(IMemeTemplateRepository repository, IBlobUrlService blobUrlService)
    {
        _repository = repository;
        _blobUrlService = blobUrlService;
    }

    public async ValueTask<OperationResult<MemeTemplateResponse>> HandleAsync(GetMemeQuery query, CancellationToken cancellationToken)
    {
        var memeTemplate = await _repository.GetByIdAsync(query.Id, cancellationToken);
        
        if (memeTemplate == null)
        {
            return new OperationResult<MemeTemplateResponse>(false, null);
        }

        // Construct the full URL from the stored filename
        var fullImageUrl = _blobUrlService.GetMemeImageUrl(memeTemplate.SourceImageUrl);

        var response = new MemeTemplateResponse(
            memeTemplate.Id,
            memeTemplate.Name,
            memeTemplate.Description,
            fullImageUrl,
            memeTemplate.SourceWidth,
            memeTemplate.SourceHeight,
            memeTemplate.TextAreas.Select(ta => new MemeTextArea(
                ta.X, ta.Y, ta.Width, ta.Height,
                ta.FontFamily, ta.FontSize, ta.FontColor, ta.FontBold, ta.MaxLength, ta.BorderThickness, ta.BorderColor)).ToArray(),
            memeTemplate.CreatedAt,
            memeTemplate.UpdatedAt);

        return new OperationResult<MemeTemplateResponse>(true, response);
    }
}
