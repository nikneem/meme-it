using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games;

public sealed record EndCreativePhaseCommand(string GameCode, int RoundNumber)
    : ICommand<EndCreativePhaseResult>;
