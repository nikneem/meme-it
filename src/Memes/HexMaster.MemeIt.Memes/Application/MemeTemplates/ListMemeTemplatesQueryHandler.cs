using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Domains;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;
using HexMaster.MemeIt.Memes.Domains;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates;

/// <summary>
/// Handler for listing all meme templates.
/// </summary>
public class ListMemeTemplatesQueryHandler : IQueryHandler<ListMemeTemplatesQuery, ListMemeTemplatesResult>
{
    private readonly IMemeTemplateRepository _repository;

    public ListMemeTemplatesQueryHandler(IMemeTemplateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ListMemeTemplatesResult> HandleAsync(
        ListMemeTemplatesQuery query,
        CancellationToken cancellationToken = default)
    {
        var templates = await _repository.GetAllAsync(cancellationToken);

        var dtos = templates.Select(t => MapToDto(t)).ToList();
        return new ListMemeTemplatesResult(dtos);
    }

    private static MemeTemplateDto MapToDto(MemeTemplate template)
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
