namespace HexMaster.MemeIt.Games.Api.Requests;

/// <summary>
/// Request to rate a meme in a game round.
/// </summary>
/// <param name="MemeId">The ID of the meme template being rated.</param>
/// <param name="Rating">The rating score (0-5).</param>
public sealed record RateMemeRequest(Guid MemeId, int Rating);
