using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.GetMeme;

public class GetMemeQueryHandler : IQueryHandler<GetMemeQuery, OperationResult<MemeTemplateResponse>>
{
    private readonly IMemeTemplateRepository _repository;

    public GetMemeQueryHandler(IMemeTemplateRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<OperationResult<MemeTemplateResponse>> HandleAsync(GetMemeQuery query, CancellationToken cancellationToken)
    {
        var memeTemplate = await _repository.GetByIdAsync(query.Id, cancellationToken);
        
        if (memeTemplate == null)
        {
            return new OperationResult<MemeTemplateResponse>(false, null);
        }

        var response = new MemeTemplateResponse(
            memeTemplate.Id,
            memeTemplate.Name,
            memeTemplate.Description,
            memeTemplate.SourceImageUrl,
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
