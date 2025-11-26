namespace HexMaster.MemeIt.Games.Application.Games.EndRound;

public sealed record EndRoundResult(string GameCode, int RoundNumber, bool Success, bool IsLastRound, bool RoundEnded);
