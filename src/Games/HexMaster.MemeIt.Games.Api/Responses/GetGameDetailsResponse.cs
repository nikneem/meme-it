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
public sealed record GetGameDetailsResponse(
    string GameCode,
    string State,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<PlayerDetailsDto> Players,
    IReadOnlyCollection<RoundDetailsDto> Rounds,
    bool IsAdmin);

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
