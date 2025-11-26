namespace HexMaster.MemeIt.Games.Abstractions.Domains;

public interface IMemeSubmissionScore
{
    Guid PlayerId { get; }
    int Rating { get; }
}