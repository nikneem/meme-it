using System;
using System.Linq;
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
/// Handles ending the creative phase and starting the score phase.
/// </summary>
public sealed class EndCreativePhaseCommandHandler : ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>
{
    private readonly IGamesRepository _repository;
    private readonly DaprClient _daprClient;
    private readonly IScheduledTaskService _scheduledTaskService;
    private readonly ILogger<EndCreativePhaseCommandHandler> _logger;

    public EndCreativePhaseCommandHandler(
        IGamesRepository repository,
        DaprClient daprClient,
        IScheduledTaskService scheduledTaskService,
        ILogger<EndCreativePhaseCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _scheduledTaskService = scheduledTaskService ?? throw new ArgumentNullException(nameof(scheduledTaskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EndCreativePhaseResult> HandleAsync(
        EndCreativePhaseCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        var round = game.GetRound(command.RoundNumber);
        if (round == null)
        {
            throw new InvalidOperationException($"Round {command.RoundNumber} not found in game {command.GameCode}.");
        }

        // Check if creative phase already ended - ignore duplicate requests
        if (round.CreativePhaseEnded)
        {
            _logger.LogInformation(
                "Creative phase already ended for Game={GameCode}, Round={Round}. Ignoring duplicate request.",
                command.GameCode, command.RoundNumber);
            return new EndCreativePhaseResult(game.GameCode, command.RoundNumber, false);
        }

        // Mark creative phase as ended
        game.MarkCreativePhaseEnded(command.RoundNumber);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Publish CreativePhaseEnded event
        var creativePhaseEndedEvent = new CreativePhaseEndedEvent(game.GameCode, command.RoundNumber);
        await _daprClient.PublishEventAsync(
            DaprConstants.PubSubName,
            DaprConstants.Topics.CreativePhaseEnded,
            creativePhaseEndedEvent,
            cancellationToken).ConfigureAwait(false);

        // Start score phase: pick a random unrated submission
        var firstSubmission = game.GetRandomUnratedSubmissionForRound(command.RoundNumber);
        if (firstSubmission != null)
        {
            var textEntries = firstSubmission.TextEntries
                .Select(te => new MemeTextEntryDto(te.TextFieldId, te.Value))
                .ToList();

            var scorePhaseStartedEvent = new ScorePhaseStartedEvent(
                game.GameCode,
                command.RoundNumber,
                firstSubmission.MemeId,
                firstSubmission.PlayerId,
                firstSubmission.MemeTemplateId,
                textEntries,
                RatingDurationSeconds: 30);

            await _daprClient.PublishEventAsync(
                DaprConstants.PubSubName,
                DaprConstants.Topics.ScorePhaseStarted,
                scorePhaseStartedEvent,
                cancellationToken).ConfigureAwait(false);

            // Schedule score phase end for this meme
            _scheduledTaskService.ScheduleScorePhaseEnded(
                game.GameCode,
                command.RoundNumber,
                firstSubmission.MemeTemplateId,
                delaySeconds: 30);
        }

        return new EndCreativePhaseResult(game.GameCode, command.RoundNumber, true);
    }
}
