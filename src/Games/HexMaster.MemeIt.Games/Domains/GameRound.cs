using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Domains;

/// <summary>
/// Represents a score given by a player for a meme.
/// </summary>
internal sealed record MemeScore(int RoundNumber, Guid MemeId, Guid PlayerId, int Rating);

/// <summary>
/// Concrete round state that keeps track of per-player submissions.
/// </summary>
public sealed class GameRound : IGameRound
{
    private readonly List<IMemeSubmission> _submissions = new();
    private readonly List<MemeScore> _scores = new();
    private readonly HashSet<Guid> _memesWithEndedScorePhase = new(); // Track memes whose score phase has ended

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

    public void MarkCreativePhaseEnded()
    {
        CreativePhaseEnded = true;
    }

    /// <summary>
    /// Marks the score phase as ended for the entire round.
    /// This operation is idempotent and can be called multiple times safely.
    /// </summary>
    public void MarkScorePhaseEnded()
    {
        if (ScorePhaseEnded)
        {
            return; // Already marked, no-op
        }
        ScorePhaseEnded = true;
    }

    /// <summary>
    /// Marks the score phase as ended for a specific meme.
    /// Returns true if this is the first time marking it, false if already marked.
    /// </summary>
    public bool MarkMemeScorePhaseEnded(Guid memeId)
    {
        return _memesWithEndedScorePhase.Add(memeId);
    }

    /// <summary>
    /// Checks if the score phase has ended for a specific meme.
    /// </summary>
    public bool IsMemeScorePhaseEnded(Guid memeId)
    {
        return _memesWithEndedScorePhase.Contains(memeId);
    }

    /// <summary>
    /// Adds or updates a score for a meme. Score must be between 0 and 5.
    /// Players cannot score their own memes.
    /// </summary>
    public void AddScore(Guid memeId, Guid voterId, int score)
    {
        if (score < 0 || score > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be between 0 and 5.");
        }

        var submission = _submissions.FirstOrDefault(s => s.MemeId == memeId);
        if (submission == null)
        {
            throw new InvalidOperationException($"No submission found for meme {memeId}.");
        }

        if (submission.PlayerId == voterId)
        {
            throw new InvalidOperationException("Players cannot score their own memes.");
        }

        // Remove existing score if present
        _scores.RemoveAll(s => s.MemeId == memeId && s.PlayerId == voterId);

        // Add new score
        _scores.Add(new MemeScore(RoundNumber, memeId, voterId, score));
    }

    /// <summary>
    /// Gets all scores for a specific meme.
    /// </summary>
    public IReadOnlyDictionary<Guid, int> GetScoresForMeme(Guid memeId)
    {
        return _scores
            .Where(s => s.MemeId == memeId)
            .ToDictionary(s => s.PlayerId, s => s.Rating);
    }

    /// <summary>
    /// Gets the next unscored meme for a specific voter, or null if all memes have been scored.
    /// </summary>
    public IMemeSubmission? GetNextUnscoredMeme(Guid voterId)
    {
        return _submissions.FirstOrDefault(s =>
            s.PlayerId != voterId && // Can't vote on own meme
            !_scores.Any(score => score.MemeId == s.MemeId && score.PlayerId == voterId));
    }

    /// <summary>
    /// Gets a random submission that has received no ratings yet, or null if all submissions have been rated.
    /// This ensures fair distribution of rating opportunities across all submissions.
    /// </summary>
    public IMemeSubmission? GetRandomUnratedSubmission()
    {
        // Find submissions that have no ratings at all (no entry in scores collection)
        var unratedSubmissions = _submissions
            .Where(s => !_scores.Any(score => score.MemeId == s.MemeId))
            .ToList();

        if (unratedSubmissions.Count == 0)
        {
            return null;
        }

        // Return a random unrated submission
        var random = new Random();
        var randomIndex = random.Next(unratedSubmissions.Count);
        return unratedSubmissions[randomIndex];
    }
}
