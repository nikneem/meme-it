using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Domains;

public class MemeSubmissionScore(Guid playerId, int rating) : IMemeSubmissionScore
{
    public Guid PlayerId { get; } = playerId;
    public int Rating { get; } = rating;
}