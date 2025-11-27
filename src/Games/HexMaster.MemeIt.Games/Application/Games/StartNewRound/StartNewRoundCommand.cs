using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games.StartNewRound;

public sealed record StartNewRoundCommand(string GameCode, int RoundNumber)
    : ICommand<StartNewRoundResult>;
