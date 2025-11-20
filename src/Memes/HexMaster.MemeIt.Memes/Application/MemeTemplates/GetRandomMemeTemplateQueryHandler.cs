using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Domains;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;
using HexMaster.MemeIt.Memes.Domains;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates;

/// <summary>
/// Handler for getting a random meme template.
/// </summary>
public class GetRandomMemeTemplateQueryHandler : IQueryHandler<GetRandomMemeTemplateQuery, GetRandomMemeTemplateResult>
{
    private readonly IMemeTemplateRepository _repository;

    public GetRandomMemeTemplateQueryHandler(IMemeTemplateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<GetRandomMemeTemplateResult> HandleAsync(
        GetRandomMemeTemplateQuery query,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetRandomAsync(cancellationToken);

        if (template is null)
        {
            return new GetRandomMemeTemplateResult(null);
        }

        var dto = MapToDto(template);
        return new GetRandomMemeTemplateResult(dto);
    }

    private static MemeTemplateDto MapToDto(Domains.MemeTemplate template)
    {
        return new MemeTemplateDto(
            template.Id,
            template.Title,
            template.ImageUrl,
            template.TextAreas.Select(ta => new TextAreaDefinitionDto(
                ta.X,
                ta.Y,
                ta.Width,
                ta.Height,
                ta.FontSize,
                ta.FontColor,
                ta.BorderSize,
                ta.BorderColor,
                ta.IsBold
            )).ToList(),
            template.CreatedAt,
            template.UpdatedAt
        );
    }
}
