namespace HexMaster.MemeIt.Games.Api.Requests;

/// <summary>
/// Request to submit a meme with text entries for a round.
/// </summary>
/// <param name="MemeTemplateId">The ID of the meme template.</param>
/// <param name="TextEntries">The text entries for the meme template fields.</param>
public sealed record SubmitMemeRequest(
    Guid MemeTemplateId,
    IReadOnlyCollection<TextEntryDto> TextEntries);

/// <summary>
/// Represents a text entry for a meme template field.
/// </summary>
/// <param name="TextFieldId">The ID of the text field.</param>
/// <param name="Value">The text value entered by the player.</param>
public sealed record TextEntryDto(Guid TextFieldId, string Value);
