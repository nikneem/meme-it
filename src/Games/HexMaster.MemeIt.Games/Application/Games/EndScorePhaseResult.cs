namespace HexMaster.MemeIt.Games.Application.Games;

public sealed record EndScorePhaseResult(string GameCode, int RoundNumber, bool Success, bool RoundComplete);
