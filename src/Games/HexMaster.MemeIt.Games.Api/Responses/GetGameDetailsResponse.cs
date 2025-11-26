namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response containing complete game details.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="State">The current game state.</param>
/// <param name="CreatedAt">When the game was created.</param>
/// <param name="Players">Collection of players in the game.</param>
/// <param name="Rounds">Collection of rounds played.</param>
/// <param name="IsAdmin">Whether the requesting player is the admin.</param>
/// <param name="CurrentRoundInfo">Information about the current round (null if game not started).</param>
/// <param name="PlayerSubmission">The requesting player's submission for the current round (null if not submitted).</param>
public sealed record GetGameDetailsResponse(
    string GameCode,
    string State,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<PlayerDetailsDto> Players,
    IReadOnlyCollection<RoundDetailsDto> Rounds,
    bool IsAdmin,
    CurrentRoundDetailsDto? CurrentRoundInfo,
    PlayerSubmissionDetailsDto? PlayerSubmission);

/// <summary>
/// Player details in the response.
/// </summary>
/// <param name="PlayerId">The player's unique identifier.</param>
/// <param name="DisplayName">The player's display name.</param>
/// <param name="IsReady">Whether the player is ready.</param>
public sealed record PlayerDetailsDto(Guid PlayerId, string DisplayName, bool IsReady);

/// <summary>
/// Round details in the response.
/// </summary>
/// <param name="RoundNumber">The round number.</param>
/// <param name="SubmissionCount">Number of submissions received.</param>
public sealed record RoundDetailsDto(int RoundNumber, int SubmissionCount);

/// <summary>
/// Current round information in the response.
/// </summary>
/// <param name="RoundNumber">The round number.</param>
/// <param name="StartedAt">When the round started.</param>
/// <param name="Phase">The current phase (Creative, Scoring, or Ended).</param>
/// <param name="CreativePhaseEndTime">When the creative phase ends/ended.</param>
public sealed record CurrentRoundDetailsDto(
    int RoundNumber,
    DateTimeOffset StartedAt,
    string Phase,
    DateTimeOffset? CreativePhaseEndTime);

/// <summary>
/// Player submission details in the response.
/// </summary>
/// <param name="MemeTemplateId">The selected meme template ID.</param>
/// <param name="TextEntries">The text entries submitted by the player.</param>
/// <param name="SubmittedAt">When the submission was made.</param>
public sealed record PlayerSubmissionDetailsDto(
    string MemeTemplateId,
    IReadOnlyCollection<TextEntryDetailsDto> TextEntries,
    DateTimeOffset SubmittedAt);

/// <summary>
/// Text entry details in the response.
/// </summary>
/// <param name="TextFieldId">The text field identifier.</param>
/// <param name="Value">The text value.</param>
public sealed record TextEntryDetailsDto(string TextFieldId, string Value);
