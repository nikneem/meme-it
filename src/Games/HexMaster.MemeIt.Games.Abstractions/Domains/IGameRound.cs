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
    bool CreativePhaseEnded { get; }

    /// <summary>
    /// Indicates whether the score phase has ended for this round.
    /// </summary>
    bool ScorePhaseEnded { get; }

    /// <summary>
    /// Gets all scores for a specific meme.
    /// </summary>
    /// <param name="memeId">The meme template ID.</param>
    /// <returns>Dictionary of voter ID to score.</returns>
    IReadOnlyDictionary<Guid, int> GetScoresForMeme(Guid memeId);

    /// <summary>
    /// Gets the next unscored meme for a specific voter, or null if all memes have been scored.
    /// </summary>
    /// <param name="voterId">The voter's player ID.</param>
    /// <returns>The next unscored meme submission, or null.</returns>
    IMemeSubmission? GetNextUnscoredMeme(Guid voterId);

    /// <summary>
    /// Checks if the score phase has ended for a specific meme.
    /// </summary>
    /// <param name="memeId">The meme template ID.</param>
    /// <returns>True if the score phase has ended for this meme, false otherwise.</returns>
    bool IsMemeScorePhaseEnded(Guid memeId);
}
