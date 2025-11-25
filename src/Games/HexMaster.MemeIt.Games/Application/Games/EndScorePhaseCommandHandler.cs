using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
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

        // Check if there are more memes to score (pick any player as reference - we need unscored memes for anyone)
        var anyPlayer = game.Players.FirstOrDefault();
        if (anyPlayer == null)
        {
            throw new InvalidOperationException($"No players found in game {command.GameCode}.");
        }

        var nextMeme = round.GetNextUnscoredMeme(anyPlayer.PlayerId);

        if (nextMeme != null)
        {
            // Still have memes to score, start scoring the next one
            _logger.LogInformation(
                "Found unscored meme {MemeId} in round {RoundNumber} of game {GameCode}. Starting score phase for it.",
                nextMeme.MemeTemplateId, command.RoundNumber, command.GameCode);

            var textEntries = nextMeme.TextEntries
                .Select(te => new MemeTextEntryDto(te.TextFieldId, te.Value))
                .ToList();

            var scorePhaseStartedEvent = new ScorePhaseStartedEvent(
                game.GameCode,
                command.RoundNumber,
                nextMeme.MemeTemplateId,
                nextMeme.PlayerId,
                nextMeme.MemeTemplateId,
                textEntries);

            await _daprClient.PublishEventAsync(
                "chatservice-pubsub",
                "scorephasestarted",
                scorePhaseStartedEvent,
                cancellationToken).ConfigureAwait(false);

            // Schedule score phase end for this next meme
            _scheduledTaskService.ScheduleScorePhaseEnded(
                game.GameCode,
                command.RoundNumber,
                nextMeme.MemeTemplateId,
                delaySeconds: 30);

            await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

            return new EndScorePhaseResult(game.GameCode, command.RoundNumber, true, false);
        }
        else
        {
            // No more memes to score, mark score phase as ended
            _logger.LogInformation(
                "All memes scored in round {RoundNumber} of game {GameCode}. Ending score phase.",
                command.RoundNumber, command.GameCode);

            game.MarkScorePhaseEnded(command.RoundNumber);

            await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

            // TODO: Publish RoundEndedEvent and schedule next round or game end

            return new EndScorePhaseResult(game.GameCode, command.RoundNumber, true, true);
        }
    }
}
