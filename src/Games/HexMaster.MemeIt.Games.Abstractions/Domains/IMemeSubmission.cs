namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Captures a player's meme selection and the accompanying text entries for a round.
/// </summary>
public interface IMemeSubmission
{
    /// <summary>
    /// Unique identifier for this specific meme submission.
    /// Multiple players can use the same MemeTemplateId, so this ID distinguishes individual submissions.
    /// </summary>
    Guid SubmissionId { get; }

    /// <summary>
    /// Identifier of the player who created the submission.
    /// </summary>
    Guid PlayerId { get; }

    /// <summary>
    /// Identifier of the meme template selected for this submission.
    /// </summary>
    Guid MemeTemplateId { get; }

    bool HasScorePhaseStarted { get; }
    bool HasScorePhaseEnded { get; }

    int TotalScore { get; }
    
    /// <summary>
    /// Text entries keyed by template text field identifiers.
    /// </summary>
    IReadOnlyCollection<IMemeTextEntry> TextEntries { get; }

    IReadOnlyCollection<IMemeSubmissionScore> Scores { get; }

    public void AddScore(Guid playerId, int rating);
    public void RemoveScore(Guid playerId);


    void StartScorePhase();
    void EndScorePhase();
}
