using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates;

/// <summary>
/// Handler for creating a new meme template.
/// </summary>
public class CreateMemeTemplateCommandHandler : ICommandHandler<CreateMemeTemplateCommand, CreateMemeTemplateResult>
{
    private readonly IMemeTemplateRepository _repository;

    public CreateMemeTemplateCommandHandler(IMemeTemplateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<CreateMemeTemplateResult> HandleAsync(
        CreateMemeTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            // Map DTOs to value objects
            var textAreas = command.TextAreas.Select(dto =>
                TextAreaDefinition.Create(
                    dto.X,
                    dto.Y,
                    dto.Width,
                    dto.Height,
                    dto.FontSize,
                    dto.FontColor,
                    dto.BorderSize,
                    dto.BorderColor,
                    dto.IsBold
                )
            ).ToList();

            // Create domain entity
            var template = MemeTemplate.Create(command.Title, command.ImageUrl, textAreas);

            // Persist
            var id = await _repository.AddAsync(template, cancellationToken);

            return new CreateMemeTemplateResult(id);
        }
        catch (DomainException ex)
        {
            // In a real application, you might want to handle domain exceptions differently
            // For now, let them bubble up to be caught by the API layer
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}
