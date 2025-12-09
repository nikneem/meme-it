using System.Diagnostics;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Observability;
using HexMaster.MemeIt.Games.Constants;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents;
using HexMaster.MemeIt.IntegrationEvents.Events;

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
    private readonly DaprClient _daprClient;

    public CreateGameCommandHandler(
        IGamesRepository repository,
        IGameCodeGenerator codeGenerator,
        TimeProvider timeProvider,
        GamesMetrics metrics,
        DaprClient daprClient)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
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

            // Publish NewGameStartedEvent if a previous game code was provided
            if (!string.IsNullOrWhiteSpace(command.PreviousGameCode))
            {
                var previousGame = await _repository.GetByGameCodeAsync(command.PreviousGameCode, cancellationToken).ConfigureAwait(false);
                if (previousGame is not null)
                {
                    var newGameStartedEvent = new NewGameStartedEvent(
                        command.PreviousGameCode,
                        game.GameCode,
                        command.PlayerDisplayName);

                    await _daprClient.PublishEventAsync(
                        DaprConstants.PubSubName,
                        DaprConstants.Topics.NewGameStarted,
                        newGameStartedEvent,
                        cancellationToken).ConfigureAwait(false);
                }
            }

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
