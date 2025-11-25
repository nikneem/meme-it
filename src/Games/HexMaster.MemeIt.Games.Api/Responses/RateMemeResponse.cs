namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response returned after rating a meme.
/// </summary>
/// <param name="Success">Whether the rating was successfully recorded.</param>
public sealed record RateMemeResponse(bool Success);
