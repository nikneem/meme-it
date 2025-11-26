using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games;

public sealed record EndScorePhaseCommand(string GameCode, int RoundNumber, Guid SubmissionId)
    : ICommand<EndScorePhaseResult>;
