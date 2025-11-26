using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games;

public sealed record RateMemeCommand(string GameCode, int RoundNumber, Guid MemeId, Guid PlayerId, int Rating)
    : ICommand<RateMemeResult>;
