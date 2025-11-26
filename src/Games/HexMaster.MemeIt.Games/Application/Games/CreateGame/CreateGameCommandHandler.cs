using System.Diagnostics;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Observability;
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
    private readonly GamesMetrics _metrics;

    public CreateGameCommandHandler(
        IGamesRepository repository,
        IGameCodeGenerator codeGenerator,
        TimeProvider timeProvider,
        GamesMetrics metrics)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
    }

    public async Task<CreateGameResult> HandleAsync(CreateGameCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = GamesActivitySource.Instance.StartActivity("CreateGame", ActivityKind.Internal);
        activity?.SetTag("game.player_id", command.PlayerId);
        activity?.SetTag("game.has_password", !string.IsNullOrEmpty(command.Password));

        var stopwatch = Stopwatch.StartNew();

        try
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

            activity?.SetTag("game.code", game.GameCode);

            await _repository.CreateAsync(game, cancellationToken).ConfigureAwait(false);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordGameCreated();
            _metrics.RecordHandlerDuration("CreateGame", stopwatch.Elapsed.TotalMilliseconds, success: true);

            return new CreateGameResult(game.GameCode, game.AdminPlayerId, game.State, game.CreatedAt);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _metrics.RecordHandlerDuration("CreateGame", stopwatch.Elapsed.TotalMilliseconds, success: false);
            _metrics.RecordCommandFailed("CreateGame", ex.GetType().Name);
            throw;
        }
    }
}
