namespace HexMaster.MemeIt.Games.Application.Games.SelectMemeTemplate;

/// <summary>
/// Result returned after selecting a meme template.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="PlayerId">The player ID.</param>
/// <param name="RoundNumber">The round number.</param>
/// <param name="MemeTemplateId">The selected meme template ID.</param>
public sealed record SelectMemeTemplateResult(string GameCode, Guid PlayerId, int RoundNumber, Guid MemeTemplateId);
