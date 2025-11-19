namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Represents a single text value keyed to a meme template text field.
/// </summary>
public interface IMemeTextEntry
{
    /// <summary>
    /// Identifier of the text field as defined by the meme template.
    /// </summary>
    string TextFieldId { get; }

    /// <summary>
    /// Text entered by the player for the given field.
    /// </summary>
    string Value { get; }
}
