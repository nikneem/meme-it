using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents;
using HexMaster.MemeIt.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles ending the score phase for a specific meme and starting the next meme's score phase or ending the round.
/// </summary>
public sealed class EndScorePhaseCommandHandler : ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult>
{
    private readonly IGamesRepository _repository;
    private readonly DaprClient _daprClient;
    private readonly IScheduledTaskService _scheduledTaskService;
    private readonly ILogger<EndScorePhaseCommandHandler> _logger;

    public EndScorePhaseCommandHandler(
        IGamesRepository repository,
        DaprClient daprClient,
        IScheduledTaskService scheduledTaskService,
        ILogger<EndScorePhaseCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _scheduledTaskService = scheduledTaskService ?? throw new ArgumentNullException(nameof(scheduledTaskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EndScorePhaseResult> HandleAsync(
        EndScorePhaseCommand command,
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

        // Check if this meme's score phase has already ended
        if (round.IsMemeScorePhaseEnded(command.MemeId))
        {
            _logger.LogInformation(
                "Score phase for meme {MemeId} in round {RoundNumber} of game {GameCode} already ended. Ignoring duplicate request.",
                command.MemeId, command.RoundNumber, command.GameCode);
            return new EndScorePhaseResult(game.GameCode, command.RoundNumber, false, false);
        }

        // Mark this meme's score phase as ended (cast to concrete type to access internal method)
        if (round is GameRound concreteRound)
        {
            concreteRound.MarkMemeScorePhaseEnded(command.MemeId);
        }

        // Check if there are more unrated submissions
        var nextSubmission = game.GetRandomUnratedSubmissionForRound(command.RoundNumber);

        if (nextSubmission != null)
        {
            // Still have unrated submissions, start scoring the next one
            _logger.LogInformation(
                "Found unrated submission {MemeId} in round {RoundNumber} of game {GameCode}.",
                nextSubmission.MemeTemplateId, command.RoundNumber, command.GameCode);

            var textEntries = nextSubmission.TextEntries
                .Select(te => new MemeTextEntryDto(te.TextFieldId, te.Value))
                .ToList();

            var scorePhaseStartedEvent = new ScorePhaseStartedEvent(
                game.GameCode,
                command.RoundNumber,
                nextSubmission.MemeTemplateId,
                nextSubmission.PlayerId,
                nextSubmission.MemeTemplateId,
                textEntries,
                RatingDurationSeconds: 30);

            await _daprClient.PublishEventAsync(
                DaprConstants.PubSubName,
                DaprConstants.Topics.ScorePhaseStarted,
                scorePhaseStartedEvent,
                cancellationToken).ConfigureAwait(false);

            // Schedule score phase end for this next meme
            _scheduledTaskService.ScheduleScorePhaseEnded(
                game.GameCode,
                command.RoundNumber,
                nextSubmission.MemeTemplateId,
                delaySeconds: 30);

            await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

            return new EndScorePhaseResult(game.GameCode, command.RoundNumber, true, false);
        }
        else
        {
            // Last meme scored or no more memes, mark score phase as ended and schedule round end
            _logger.LogInformation(
                "Last meme scored in round {RoundNumber} of game {GameCode}. Scheduling round end.",
                command.RoundNumber, command.GameCode);

            game.MarkScorePhaseEnded(command.RoundNumber);

            // Schedule RoundEnded task for 10 seconds (scoreboard will be calculated there)
            _scheduledTaskService.ScheduleRoundEnded(
                game.GameCode,
                command.RoundNumber,
                delaySeconds: 10);

            await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Round {RoundNumber} score phase ended for game {GameCode}. RoundEnded task scheduled.",
                command.RoundNumber, game.GameCode);

            return new EndScorePhaseResult(game.GameCode, command.RoundNumber, true, true);
        }
    }
}
