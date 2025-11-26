namespace HexMaster.MemeIt.Games.Abstractions.Application.Games;

/// <summary>
/// Result containing full game details.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="State">The current game state.</param>
/// <param name="CreatedAt">When the game was created.</param>
/// <param name="Players">Collection of players in the game.</param>
/// <param name="Rounds">Collection of rounds played.</param>
/// <param name="IsAdmin">Whether the requesting player is the admin.</param>
/// <param name="CurrentRoundInfo">Information about the current round (null if game not started).</param>
/// <param name="PlayerSubmission">The requesting player's submission for the current round (null if not submitted).</param>
public sealed record GetGameDetailsResult(
    string GameCode,
    string State,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<GamePlayerDto> Players,
    IReadOnlyCollection<GameRoundDto> Rounds,
    bool IsAdmin,
    CurrentRoundInfoDto? CurrentRoundInfo,
    PlayerSubmissionDto? PlayerSubmission);

/// <summary>
/// Player details within a game.
/// </summary>
/// <param name="PlayerId">The player's unique identifier.</param>
/// <param name="DisplayName">The player's display name.</param>
/// <param name="IsReady">Whether the player is ready (lobby state).</param>
public sealed record GamePlayerDto(Guid PlayerId, string DisplayName, bool IsReady);

/// <summary>
/// Round details within a game.
/// </summary>
/// <param name="RoundNumber">The round number.</param>
/// <param name="SubmissionCount">Number of submissions received.</param>
public sealed record GameRoundDto(int RoundNumber, int SubmissionCount);

/// <summary>
/// Information about the current round.
/// </summary>
/// <param name="RoundNumber">The round number.</param>
/// <param name="StartedAt">When the round started.</param>
/// <param name="Phase">The current phase (Creative, Scoring, or Ended).</param>
/// <param name="CreativePhaseEndTime">When the creative phase ends/ended.</param>
public sealed record CurrentRoundInfoDto(
    int RoundNumber,
    DateTimeOffset StartedAt,
    string Phase,
    DateTimeOffset? CreativePhaseEndTime);

/// <summary>
/// Player's submission for the current round.
/// </summary>
/// <param name="MemeTemplateId">The selected meme template ID.</param>
/// <param name="TextEntries">The text entries submitted by the player.</param>
/// <param name="SubmittedAt">When the submission was made.</param>
public sealed record PlayerSubmissionDto(
    string MemeTemplateId,
    IReadOnlyCollection<TextEntryDto> TextEntries,
    DateTimeOffset SubmittedAt);

/// <summary>
/// A text entry for a meme.
/// </summary>
/// <param name="TextFieldId">The text field identifier.</param>
/// <param name="Value">The text value.</param>
public sealed record TextEntryDto(string TextFieldId, string Value);
