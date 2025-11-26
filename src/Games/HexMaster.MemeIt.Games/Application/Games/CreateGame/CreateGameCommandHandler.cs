using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Application.Games.CreateGame;

/// <summary>
/// Creates a new game aggregate, sets up the admin player, and persists it.
/// </summary>
public sealed class CreateGameCommandHandler : ICommandHandler<CreateGameCommand, CreateGameResult>
{
    private readonly IGamesRepository _repository;
    private readonly IGameCodeGenerator _codeGenerator;
    private readonly TimeProvider _timeProvider;

    public CreateGameCommandHandler(
        IGamesRepository repository,
        IGameCodeGenerator codeGenerator,
        TimeProvider timeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<CreateGameResult> HandleAsync(CreateGameCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var code = _codeGenerator.Generate();
        if (string.IsNullOrWhiteSpace(command.PlayerDisplayName))
        {
            throw new ArgumentException("A display name is required to create a game.", nameof(command));
        }

        var createdAt = _timeProvider.GetUtcNow();
        var game = new Game(
            code,
            command.PlayerId,
            command.Password,
            [new GamePlayer(command.PlayerId, command.PlayerDisplayName)],
            createdAt);

        await _repository.CreateAsync(game, cancellationToken).ConfigureAwait(false);

        return new CreateGameResult(game.GameCode, game.AdminPlayerId, game.State, game.CreatedAt);
    }
}
