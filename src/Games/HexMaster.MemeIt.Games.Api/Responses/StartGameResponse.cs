namespace HexMaster.MemeIt.Games.Api.Responses;

/// <summary>
/// Response returned after starting a game.
/// </summary>
/// <param name="GameCode">The game code that was started.</param>
/// <param name="RoundNumber">The current round number.</param>
public sealed record StartGameResponse(string GameCode, int RoundNumber);
