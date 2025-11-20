using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Domains;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;
using HexMaster.MemeIt.Memes.Domains;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates;

/// <summary>
/// Handler for getting a meme template by ID.
/// </summary>
public class GetMemeTemplateByIdQueryHandler : IQueryHandler<GetMemeTemplateByIdQuery, GetMemeTemplateByIdResult>
{
    private readonly IMemeTemplateRepository _repository;

    public GetMemeTemplateByIdQueryHandler(IMemeTemplateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<GetMemeTemplateByIdResult> HandleAsync(
        GetMemeTemplateByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByIdAsync(query.Id, cancellationToken);

        if (template is null)
        {
            return new GetMemeTemplateByIdResult(null);
        }

        var dto = MapToDto(template);
        return new GetMemeTemplateByIdResult(dto);
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
