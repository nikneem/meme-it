using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Repositories;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates;

/// <summary>
/// Handler for deleting a meme template.
/// </summary>
public class DeleteMemeTemplateCommandHandler : ICommandHandler<DeleteMemeTemplateCommand, DeleteMemeTemplateResult>
{
    private readonly IMemeTemplateRepository _repository;

    public DeleteMemeTemplateCommandHandler(IMemeTemplateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<DeleteMemeTemplateResult> HandleAsync(
        DeleteMemeTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var exists = await _repository.ExistsAsync(command.Id, cancellationToken);
        if (!exists)
        {
            return new DeleteMemeTemplateResult(false);
        }

        await _repository.DeleteAsync(command.Id, cancellationToken);
        return new DeleteMemeTemplateResult(true);
    }
}
