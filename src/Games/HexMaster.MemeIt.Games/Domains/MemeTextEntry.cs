using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Domains;

/// <summary>
/// Concrete immutable implementation of <see cref="IMemeTextEntry"/>.
/// </summary>
public sealed class MemeTextEntry : IMemeTextEntry
{
    public MemeTextEntry(Guid textFieldId, string value)
    {
        TextFieldId = textFieldId != Guid.Empty
            ? textFieldId
            : throw new ArgumentException("Text field id must be provided", nameof(textFieldId));

        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Guid TextFieldId { get; }

    public string Value { get; }
}
