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
public sealed class EndCreativePhaseCommandHandler(
    IGamesRepository repository,
    DaprClient daprClient,
    IScheduledTaskService scheduledTaskService,
    ILogger<EndCreativePhaseCommandHandler> logger)
    : ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>
{
    private readonly IGamesRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly DaprClient _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    private readonly IScheduledTaskService _scheduledTaskService = scheduledTaskService ?? throw new ArgumentNullException(nameof(scheduledTaskService));
    private readonly ILogger<EndCreativePhaseCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
        if (round.HasCreativePhaseEnded)
        {
            _logger.LogInformation(
                "Creative phase already ended for Game={GameCode}, Round={Round}. Ignoring duplicate request.",
                command.GameCode, command.RoundNumber);
            return new EndCreativePhaseResult(game.GameCode, command.RoundNumber, false);
        }

        // Mark creative phase as ended
        game.MarkCreativePhaseEnded(command.RoundNumber);
        // Start score phase: pick a random unrated submission, this also updates the state so we need to save the game after this
        var randomSubmission = game.GetRandomUnratedSubmissionForRound(command.RoundNumber);
        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Publish HasCreativePhaseEnded event
        var creativePhaseEndedEvent = new CreativePhaseEndedEvent(game.GameCode, command.RoundNumber);
        await _daprClient.PublishEventAsync(
            DaprConstants.PubSubName,
            DaprConstants.Topics.CreativePhaseEnded,
            creativePhaseEndedEvent,
            cancellationToken).ConfigureAwait(false);

        if (randomSubmission != null)
        {
            var textEntries = randomSubmission.TextEntries
                .Select(te => new MemeTextEntryDto(te.TextFieldId, te.Value))
                .ToList();

            var scorePhaseStartedEvent = new ScorePhaseStartedEvent(
                game.GameCode,
                command.RoundNumber,
                randomSubmission.SubmissionId,
                randomSubmission.PlayerId,
                randomSubmission.MemeTemplateId,
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
                randomSubmission.SubmissionId,
                delaySeconds: 30);
        }

        return new EndCreativePhaseResult(game.GameCode, command.RoundNumber, true);
    }
}
