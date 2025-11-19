using System;
using System.Collections.Generic;
using System.Linq;
using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Domains;

/// <summary>
/// Concrete round state that keeps track of per-player submissions.
/// </summary>
public sealed class GameRound : IGameRound
{
    private readonly List<IMemeSubmission> _submissions = new();

    public GameRound(int roundNumber)
    {
        if (roundNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), roundNumber, "Round numbers start at 1.");
        }

        RoundNumber = roundNumber;
    }

    public int RoundNumber { get; }

    public IReadOnlyCollection<IMemeSubmission> Submissions => _submissions.AsReadOnly();

    internal void UpsertSubmission(IMemeSubmission submission)
    {
        var normalized = MemeSubmission.From(submission);
        var index = _submissions.FindIndex(s => s.PlayerId == normalized.PlayerId);
        if (index >= 0)
        {
            _submissions[index] = normalized;
        }
        else
        {
            _submissions.Add(normalized);
        }
    }

    internal void RemoveSubmissionForPlayer(Guid playerId)
    {
        _submissions.RemoveAll(s => s.PlayerId == playerId);
    }
}
