using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates;

/// <summary>
/// Handler for updating an existing meme template.
/// </summary>
public class UpdateMemeTemplateCommandHandler : ICommandHandler<UpdateMemeTemplateCommand, UpdateMemeTemplateResult>
{
    private readonly IMemeTemplateRepository _repository;

    public UpdateMemeTemplateCommandHandler(IMemeTemplateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<UpdateMemeTemplateResult> HandleAsync(
        UpdateMemeTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            // Load existing template
            var template = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (template is null)
            {
                return new UpdateMemeTemplateResult(false);
            }

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

            // Update domain entity
            template.Update(command.Title, command.ImageUrl, command.Width, command.Height, textAreas);

            // Persist
            await _repository.UpdateAsync(template, cancellationToken);

            return new UpdateMemeTemplateResult(true);
        }
        catch (DomainException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}
