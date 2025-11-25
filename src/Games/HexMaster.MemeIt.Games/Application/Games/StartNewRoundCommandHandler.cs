using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.IntegrationEvents;
using HexMaster.MemeIt.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles starting a new round by creating the round and publishing the RoundStartedEvent.
/// </summary>
public sealed class StartNewRoundCommandHandler : ICommandHandler<StartNewRoundCommand, StartNewRoundResult>
{
    private readonly IGamesRepository _repository;
    private readonly DaprClient _daprClient;
    private readonly IScheduledTaskService _scheduledTaskService;
    private readonly ILogger<StartNewRoundCommandHandler> _logger;

    public StartNewRoundCommandHandler(
        IGamesRepository repository,
        DaprClient daprClient,
        IScheduledTaskService scheduledTaskService,
        ILogger<StartNewRoundCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _scheduledTaskService = scheduledTaskService ?? throw new ArgumentNullException(nameof(scheduledTaskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StartNewRoundResult> HandleAsync(
        StartNewRoundCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        // Verify the expected round number matches
        if (game.CurrentRound + 1 != command.RoundNumber)
        {
            _logger.LogWarning(
                "Round number mismatch for game {GameCode}. Expected {Expected}, got {Actual}",
                command.GameCode, game.CurrentRound + 1, command.RoundNumber);

            throw new InvalidOperationException(
                $"Cannot start round {command.RoundNumber}. Current round is {game.CurrentRound}.");
        }

        // Create the next round
        var round = game.NextRound();

        // Persist the updated game
        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Schedule the creative phase end task (30 seconds from now)
        _scheduledTaskService.ScheduleCreativePhaseEnded(game.GameCode, round.RoundNumber, delaySeconds: 30);

        // Publish RoundStartedEvent
        var roundStartedEvent = new RoundStartedEvent(game.GameCode, round.RoundNumber);
        await _daprClient.PublishEventAsync(
            DaprConstants.PubSubName,
            DaprConstants.Topics.RoundStarted,
            roundStartedEvent,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Started new round {RoundNumber} for game {GameCode}",
            round.RoundNumber, game.GameCode);

        return new StartNewRoundResult(game.GameCode, round.RoundNumber);
    }
}
