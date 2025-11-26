namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Describes the state of a single round within a game.
/// </summary>
public interface IGameRound
{
    /// <summary>
    /// Sequential round number starting at 1.
    /// </summary>
    int RoundNumber { get; }

    /// <summary>
    /// The date and time when this round started.
    /// </summary>
    DateTimeOffset StartedAt { get; }

    /// <summary>
    /// Per-player meme submissions collected for this round.
    /// </summary>
    IReadOnlyCollection<IMemeSubmission> Submissions { get; }

    /// <summary>
    /// Indicates whether the creative phase has ended for this round.
    /// </summary>
    bool HasCreativePhaseEnded { get; }

    /// <summary>
    /// Indicates whether the score phase has ended for this round.
    /// </summary>
    bool HasScorePhaseEnded { get; }

    bool HasClosedRound { get;  }

    /// <summary>
    /// Gets all scores for a specific meme.
    /// </summary>
    /// <param name="memeId">The meme template ID.</param>
    /// <returns>Dictionary of voter ID to score.</returns>
    IReadOnlyDictionary<Guid, int> GetScoresForSubmission(Guid memeId);

    /// <summary>
    /// Checks if the score phase has ended for a specific meme.
    /// </summary>
    /// <param name="submissionId"></param>
    /// <returns>True if the score phase has ended for this meme, false otherwise.</returns>
    bool HasScoringPhaseBeenEnded(Guid submissionId);

    void AddScore(Guid submissionId, Guid playerId, int score);
    void MarkMemeScorePhaseEnded(Guid submissionId);
    void MarkRoundClosed();

    IMemeSubmission? GetRandomUnratedSubmission();
}
