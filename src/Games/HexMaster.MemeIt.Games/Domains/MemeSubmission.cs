using System;
using System.Collections.Generic;
using System.Linq;
using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Domains;

/// <summary>
/// Concrete meme submission that stores selected template and entered texts.
/// </summary>
public sealed class MemeSubmission : IMemeSubmission
{
    private readonly List<IMemeTextEntry> _textEntries;

    public MemeSubmission(Guid playerId, Guid memeTemplateId, IEnumerable<IMemeTextEntry> textEntries)
        : this(Guid.NewGuid(), playerId, memeTemplateId, textEntries)
    {
    }

    public MemeSubmission(Guid memeId, Guid playerId, Guid memeTemplateId, IEnumerable<IMemeTextEntry> textEntries)
    {
        MemeId = memeId != Guid.Empty
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

    public Guid MemeId { get; }

    public Guid PlayerId { get; }

    public Guid MemeTemplateId { get; }

    public IReadOnlyCollection<IMemeTextEntry> TextEntries => _textEntries.AsReadOnly();

    internal static MemeSubmission From(IMemeSubmission submission)
        => submission is MemeSubmission concrete
            ? concrete
            : new MemeSubmission(submission.MemeId, submission.PlayerId, submission.MemeTemplateId, submission.TextEntries);
}
