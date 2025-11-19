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
    {
        PlayerId = playerId != Guid.Empty
            ? playerId
            : throw new ArgumentException("Player id must be provided", nameof(playerId));

        MemeTemplateId = memeTemplateId != Guid.Empty
            ? memeTemplateId
            : throw new ArgumentException("Meme template id must be provided", nameof(memeTemplateId));

        _textEntries = textEntries?.ToList() ?? new List<IMemeTextEntry>();
    }

    public Guid PlayerId { get; }

    public Guid MemeTemplateId { get; }

    public IReadOnlyCollection<IMemeTextEntry> TextEntries => _textEntries.AsReadOnly();

    internal static MemeSubmission From(IMemeSubmission submission)
        => submission is MemeSubmission concrete
            ? concrete
            : new MemeSubmission(submission.PlayerId, submission.MemeTemplateId, submission.TextEntries);
}
