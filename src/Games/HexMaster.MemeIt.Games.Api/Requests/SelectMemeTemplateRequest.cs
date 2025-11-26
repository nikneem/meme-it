namespace HexMaster.MemeIt.Games.Api.Requests;

/// <summary>
/// Request to select a meme template for a round.
/// </summary>
/// <param name="MemeTemplateId">The ID of the selected meme template.</param>
public sealed record SelectMemeTemplateRequest(Guid MemeTemplateId);
