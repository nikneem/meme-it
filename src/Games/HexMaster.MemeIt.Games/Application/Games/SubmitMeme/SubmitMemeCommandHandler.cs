using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games.EndCreativePhase;
using HexMaster.MemeIt.Games.Domains;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Games.SubmitMeme;

/// <summary>
/// Handles submitting a meme with text entries for a player in a round.
/// </summary>
public sealed class SubmitMemeCommandHandler : ICommandHandler<SubmitMemeCommand, SubmitMemeResult>
{
    private readonly IGamesRepository _repository;
    private readonly ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult> _endCreativePhaseHandler;
    private readonly ILogger<SubmitMemeCommandHandler> _logger;

    public SubmitMemeCommandHandler(
        IGamesRepository repository,
        ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult> endCreativePhaseHandler,
        ILogger<SubmitMemeCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _endCreativePhaseHandler = endCreativePhaseHandler ?? throw new ArgumentNullException(nameof(endCreativePhaseHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SubmitMemeResult> HandleAsync(
        SubmitMemeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        // Verify player is part of the game
        if (!game.Players.Any(p => p.PlayerId == command.PlayerId))
        {
            throw new InvalidOperationException("Player is not part of this game.");
        }

        // Map DTOs to domain MemeTextEntry objects
        var textEntries = command.TextEntries
            .Select(dto => new MemeTextEntry(dto.TextFieldId, dto.Value))
            .ToArray();

        // Create submission with meme template and text entries
        var submission = new MemeSubmission(
            command.PlayerId,
            command.MemeTemplateId,
            textEntries);

        game.AddMemeSubmission(command.RoundNumber, submission);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Check if all players have submitted their memes
        var round = game.GetRound(command.RoundNumber);
        if (round != null && !round.HasCreativePhaseEnded)
        {
            var totalPlayers = game.Players.Count;
            var totalValidSubmissions = round.Submissions.Count(s => s.TextEntries.Any());

            if (totalValidSubmissions >= totalPlayers)
            {
                _logger.LogInformation(
                    "All {TotalPlayers} players have submitted valid memes for round {RoundNumber} in game {GameCode}. Ending creative phase.",
                    totalPlayers, command.RoundNumber, game.GameCode);

                // Automatically end the creative phase
                var endCreativePhaseCommand = new EndCreativePhaseCommand(game.GameCode, command.RoundNumber);
                await _endCreativePhaseHandler.HandleAsync(endCreativePhaseCommand, cancellationToken).ConfigureAwait(false);
            }
        }

        return new SubmitMemeResult(
            game.GameCode,
            command.PlayerId,
            command.RoundNumber,
            command.MemeTemplateId,
            textEntries.Length);
    }
}
