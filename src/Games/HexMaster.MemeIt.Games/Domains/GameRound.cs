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

    public bool HasCreativePhaseEnded { get; private set; }

    public bool HasRoundEnded => Submissions.All(s => s.HasScorePhaseEnded);

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
        HasCreativePhaseEnded = true;
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
    public bool HasScoringPhaseBeenEnded(Guid submissionId)
    {
        return Submissions.First(s => s.SubmissionId == submissionId).HasScorePhaseEnded;
    }

    /// <summary>
    /// Adds or updates a score for a meme. Score must be between 0 and 5.
    /// Players cannot score their own memes.
    /// </summary>
    public void AddScore(Guid submissionId, Guid playerId, int rating)
    {
        if (rating < 0 || rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), rating, "Score must be between 0 and 5.");
        }

        var submission = _submissions.FirstOrDefault(s => s.SubmissionId == submissionId);
        if (submission == null)
        {
            throw new InvalidOperationException($"No submission found for meme {submission}.");
        }

        submission.RemoveScore(playerId);
        submission.AddScore(playerId, rating);
    }

    /// <summary>
    /// Gets all scores for a specific meme.
    /// </summary>
    public IReadOnlyDictionary<Guid, int> GetScoresForSubmission(Guid memeId)
    {
        return _scores
            .Where(s => s.MemeId == memeId)
            .ToDictionary(s => s.PlayerId, s => s.Rating);
    }


    /// <summary>
    /// Gets a random submission that has received no ratings yet, or null if all submissions have been rated.
    /// This ensures fair distribution of rating opportunities across all submissions.
    /// </summary>
    public IMemeSubmission? GetRandomUnratedSubmission()
    {
        // Find submissions that have no ratings at all (no entry in scores collection)
        var unratedSubmissions = _submissions.Where(s => !s.HasScorePhaseStarted).ToList();

        if (unratedSubmissions.Count == 0)
        {
            return null;
        }

        // Return a random unrated submission
        var random = new Random();
        var randomIndex = random.Next(unratedSubmissions.Count);
        var selecedSubmission = unratedSubmissions[randomIndex];
        selecedSubmission.StartScorePhase();
                return selecedSubmission;
    }
}
