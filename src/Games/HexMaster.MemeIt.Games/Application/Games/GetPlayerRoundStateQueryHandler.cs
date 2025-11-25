using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Queries;
using HexMaster.MemeIt.Games.Abstractions.Repositories;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles the GetPlayerRoundStateQuery by retrieving the player's current round state.
/// </summary>
public sealed class GetPlayerRoundStateQueryHandler : IQueryHandler<GetPlayerRoundStateQuery, GetPlayerRoundStateResult>
{
    private readonly IGamesRepository _repository;

    public GetPlayerRoundStateQueryHandler(IGamesRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<GetPlayerRoundStateResult> HandleAsync(GetPlayerRoundStateQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var game = await _repository.GetByGameCodeAsync(query.GameCode, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{query.GameCode}' not found.");
        }

        // Verify player is part of the game
        if (!game.Players.Any(p => p.PlayerId == query.PlayerId))
        {
            throw new UnauthorizedAccessException("Player is not part of this game.");
        }

        // Get the current round
        var currentRound = game.Rounds.FirstOrDefault(r => r.RoundNumber == game.CurrentRound);
        if (currentRound == null)
        {
            throw new InvalidOperationException("No active round found.");
        }

        // Get the player's submission for this round (if any)
        var playerSubmission = currentRound.Submissions.FirstOrDefault(s => s.PlayerId == query.PlayerId);

        // Calculate creative phase end time (round start + 30 seconds)
        var creativePhaseEndTime = currentRound.StartedAt.AddSeconds(30);

        return new GetPlayerRoundStateResult(
            game.GameCode,
            query.PlayerId,
            currentRound.RoundNumber,
            currentRound.StartedAt,
            creativePhaseEndTime,
            playerSubmission?.MemeTemplateId);
    }
}
