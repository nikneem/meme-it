namespace HexMaster.MemeIt.Games.Application.Games.EndScorePhase;

public sealed record EndScorePhaseResult(string GameCode, int RoundNumber, bool Success, bool RoundComplete);
