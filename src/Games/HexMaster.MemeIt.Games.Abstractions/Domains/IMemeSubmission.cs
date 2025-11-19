using System;
using System.Collections.Generic;

namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Captures a player's meme selection and the accompanying text entries for a round.
/// </summary>
public interface IMemeSubmission
{
    /// <summary>
    /// Identifier of the player who created the submission.
    /// </summary>
    Guid PlayerId { get; }

    /// <summary>
    /// Identifier of the meme template selected for this submission.
    /// </summary>
    string MemeTemplateId { get; }

    /// <summary>
    /// Text entries keyed by template text field identifiers.
    /// </summary>
    IReadOnlyCollection<IMemeTextEntry> TextEntries { get; }
}
