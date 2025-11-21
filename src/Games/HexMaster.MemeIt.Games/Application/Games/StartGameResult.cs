namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Result returned after starting a game.
/// </summary>
/// <param name="GameCode">The game code that was started.</param>
/// <param name="RoundNumber">The current round number (should be 1).</param>
public sealed record StartGameResult(string GameCode, int RoundNumber);
