using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games.EndCreativePhase;

public sealed record EndCreativePhaseCommand(string GameCode, int RoundNumber)
    : ICommand<EndCreativePhaseResult>;
