namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response returned after selecting a meme template.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="PlayerId">The player ID.</param>
/// <param name="RoundNumber">The round number.</param>
/// <param name="MemeTemplateId">The selected meme template ID.</param>
public sealed record SelectMemeTemplateResponse(string GameCode, Guid PlayerId, int RoundNumber, Guid MemeTemplateId);
