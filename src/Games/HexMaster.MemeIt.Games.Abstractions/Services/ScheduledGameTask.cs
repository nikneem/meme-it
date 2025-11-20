namespace HexMaster.MemeIt.Games.Abstractions.Services;

public sealed class ScheduledGameTask
{
    public Guid Id { get; }
    public GameTaskType TaskType { get; }
    public string GameCode { get; }
    public int RoundNumber { get; }
    public Guid? MemeId { get; }
    public DateTimeOffset ExecuteAt { get; }

    public ScheduledGameTask(
        Guid id,
        GameTaskType taskType,
        string gameCode,
        int roundNumber,
        DateTimeOffset executeAt,
        Guid? memeId = null)
    {
        Id = id;
        TaskType = taskType;
        GameCode = gameCode ?? throw new ArgumentNullException(nameof(gameCode));
        RoundNumber = roundNumber;
        MemeId = memeId;
        ExecuteAt = executeAt;
    }
}
