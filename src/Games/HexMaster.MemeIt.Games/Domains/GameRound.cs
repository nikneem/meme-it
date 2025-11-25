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
    private readonly Dictionary<Guid, Dictionary<Guid, int>> _scores = new(); // [memeId][voterId] = score

    public GameRound(int roundNumber, DateTimeOffset? startedAt = null)
    {
        if (roundNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), roundNumber, "Round numbers start at 1.");
        }

        RoundNumber = roundNumber;
        StartedAt = startedAt ?? DateTimeOffset.UtcNow;
    }

    public int RoundNumber { get; }

    public DateTimeOffset StartedAt { get; }

    public bool CreativePhaseEnded { get; private set; }

    public bool ScorePhaseEnded { get; private set; }

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

    internal void MarkCreativePhaseEnded()
    {
        CreativePhaseEnded = true;
    }

    internal void MarkScorePhaseEnded()
    {
        ScorePhaseEnded = true;
    }

    /// <summary>
    /// Adds or updates a score for a meme. Score must be between 0 and 5.
    /// Players cannot score their own memes.
    /// </summary>
    internal void AddScore(Guid memeId, Guid voterId, int score)
    {
        if (score < 0 || score > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be between 0 and 5.");
        }

        var submission = _submissions.FirstOrDefault(s => s.MemeTemplateId == memeId);
        if (submission == null)
        {
            throw new InvalidOperationException($"No submission found for meme {memeId}.");
        }

        if (submission.PlayerId == voterId)
        {
            throw new InvalidOperationException("Players cannot score their own memes.");
        }

        if (!_scores.ContainsKey(memeId))
        {
            _scores[memeId] = new Dictionary<Guid, int>();
        }

        _scores[memeId][voterId] = score;
    }

    /// <summary>
    /// Gets all scores for a specific meme.
    /// </summary>
    public IReadOnlyDictionary<Guid, int> GetScoresForMeme(Guid memeId)
    {
        return _scores.TryGetValue(memeId, out var scores)
            ? scores
            : new Dictionary<Guid, int>();
    }

    /// <summary>
    /// Gets the next unscored meme for a specific voter, or null if all memes have been scored.
    /// </summary>
    public IMemeSubmission? GetNextUnscoredMeme(Guid voterId)
    {
        return _submissions.FirstOrDefault(s =>
            s.PlayerId != voterId && // Can't vote on own meme
            (!_scores.TryGetValue(s.MemeTemplateId, out var votes) || !votes.ContainsKey(voterId)));
    }
}
