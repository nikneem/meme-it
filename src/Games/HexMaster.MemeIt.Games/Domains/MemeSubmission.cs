using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Domains;

/// <summary>
/// Concrete meme submission that stores selected template and entered texts.
/// </summary>
public sealed class MemeSubmission : IMemeSubmission
{
    private readonly List<IMemeTextEntry> _textEntries;
    private readonly List<IMemeSubmissionScore> _scores;

    public MemeSubmission(Guid playerId, Guid memeTemplateId, IEnumerable<IMemeTextEntry> textEntries)
        : this(Guid.NewGuid(), playerId, memeTemplateId, textEntries)
    {
    }

    public MemeSubmission(Guid memeId, Guid playerId, Guid memeTemplateId, IEnumerable<IMemeTextEntry> textEntries)
    {
        SubmissionId = memeId != Guid.Empty
            ? memeId
            : throw new ArgumentException("Meme id must be provided", nameof(memeId));

        PlayerId = playerId != Guid.Empty
            ? playerId
            : throw new ArgumentException("Player id must be provided", nameof(playerId));

        MemeTemplateId = memeTemplateId != Guid.Empty
            ? memeTemplateId
            : throw new ArgumentException("Meme template id must be provided", nameof(memeTemplateId));

        _textEntries = textEntries?.ToList() ?? new List<IMemeTextEntry>();
    }

    public Guid SubmissionId { get; }

    public Guid PlayerId { get; }

    public Guid MemeTemplateId { get; }
    public bool HasScorePhaseStarted { get; private set; }
    public bool HasScorePhaseEnded { get; private set; }
    public int TotalScore => HasScorePhaseEnded ? Scores.Sum(s => s.Rating) : 0;

    public IReadOnlyCollection<IMemeTextEntry> TextEntries => _textEntries.AsReadOnly();
    public IReadOnlyCollection<IMemeSubmissionScore> Scores => _scores.AsReadOnly();
    public void AddScore(Guid playerId, int rating)
    {
        RemoveScore(playerId);
        if (Scores.All(s => s.PlayerId != playerId))
        {
            _scores.Add( new MemeSubmissionScore(playerId, rating));
        }
    }

    public void RemoveScore(Guid playerId)
    {
        // Remove all the scores by the player
        _scores.RemoveAll(s => s.PlayerId == playerId);
    }

    public void StartScorePhase()
    {
        HasScorePhaseStarted = true;
    }

    public void EndScorePhase()
    {
        HasScorePhaseEnded = true;
    }

    internal static MemeSubmission From(IMemeSubmission submission)
        => submission is MemeSubmission concrete
            ? concrete
            : new MemeSubmission(submission.SubmissionId, submission.PlayerId, submission.MemeTemplateId, submission.TextEntries);
}
