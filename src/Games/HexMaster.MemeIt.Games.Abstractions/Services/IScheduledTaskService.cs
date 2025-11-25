namespace HexMaster.MemeIt.Games.Abstractions.Services;

public interface IScheduledTaskService
{
    /// <summary>
    /// Schedules a creative phase ended task.
    /// </summary>
    /// <param name="gameCode">The game code.</param>
    /// <param name="roundNumber">The round number.</param>
    /// <param name="delaySeconds">Delay in seconds (1-120, default 30).</param>
    /// <returns>Scheduled task ID.</returns>
    Guid ScheduleCreativePhaseEnded(string gameCode, int roundNumber, int delaySeconds = 30);

    /// <summary>
    /// Schedules a score phase ended task.
    /// </summary>
    /// <param name="gameCode">The game code.</param>
    /// <param name="roundNumber">The round number.</param>
    /// <param name="memeId">The meme ID being scored.</param>
    /// <param name="delaySeconds">Delay in seconds (1-120, default 30).</param>
    /// <returns>Scheduled task ID.</returns>
    Guid ScheduleScorePhaseEnded(string gameCode, int roundNumber, Guid memeId, int delaySeconds = 30);

    /// <summary>
    /// Schedules a start new round task.
    /// </summary>
    /// <param name="gameCode">The game code.</param>
    /// <param name="roundNumber">The next round number to start.</param>
    /// <param name="delaySeconds">Delay in seconds (1-120, default 30).</param>
    /// <returns>Scheduled task ID.</returns>
    Guid ScheduleStartNewRound(string gameCode, int roundNumber, int delaySeconds = 30);

    /// <summary>
    /// Cancels a scheduled task.
    /// </summary>
    /// <param name="taskId">The task ID to cancel.</param>
    /// <returns>True if cancelled, false if not found.</returns>
    bool CancelTask(Guid taskId);

    /// <summary>
    /// Cancels all scheduled tasks for a specific game.
    /// </summary>
    /// <param name="gameCode">The game code.</param>
    /// <returns>Number of tasks cancelled.</returns>
    int CancelAllTasksForGame(string gameCode);
}
