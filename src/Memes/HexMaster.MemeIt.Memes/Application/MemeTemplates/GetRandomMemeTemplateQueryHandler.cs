using Azure.Storage.Blobs;
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
    private readonly BlobServiceClient _blobServiceClient;

    public GetRandomMemeTemplateQueryHandler(
        IMemeTemplateRepository repository,
        BlobServiceClient blobServiceClient)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
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

    private MemeTemplateDto MapToDto(Domains.MemeTemplate template)
    {
        var baseUrl = _blobServiceClient.Uri.GetLeftPart(UriPartial.Authority);
        var fullImageUrl = $"{baseUrl}{template.ImageUrl}";

        return new MemeTemplateDto(
            template.Id,
            template.Title,
            fullImageUrl,
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
