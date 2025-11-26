using HexMaster.MemeIt.Games.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Services;

public sealed class ScheduledTaskService : IScheduledTaskService, IDisposable
{
    private readonly ILogger<ScheduledTaskService> _logger;
    private readonly SortedSet<ScheduledGameTask> _taskHeap;
    private readonly Dictionary<Guid, ScheduledGameTask> _taskLookup;
    private readonly object _lock = new();
    private Timer? _timer;
    private bool _disposed;

    public event EventHandler<ScheduledGameTask>? TaskDue;

    private const int MinDelaySeconds = 1;
    private const int MaxDelaySeconds = 120;
    private const int DefaultDelaySeconds = 30;

    public ScheduledTaskService(ILogger<ScheduledTaskService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _taskHeap = new SortedSet<ScheduledGameTask>(Comparer<ScheduledGameTask>.Create((a, b) =>
        {
            var cmp = a.ExecuteAt.CompareTo(b.ExecuteAt);
            return cmp != 0 ? cmp : a.Id.CompareTo(b.Id);
        }));
        _taskLookup = new Dictionary<Guid, ScheduledGameTask>();
    }

    public Guid ScheduleCreativePhaseEnded(string gameCode, int roundNumber, int delaySeconds = DefaultDelaySeconds)
    {
        ValidateDelay(ref delaySeconds);
        var task = new ScheduledGameTask(
            Guid.NewGuid(),
            GameTaskType.CreativePhaseEnded,
            gameCode,
            roundNumber,
            DateTimeOffset.UtcNow.AddSeconds(delaySeconds));

        ScheduleTask(task);
        _logger.LogInformation(
            "Scheduled HasCreativePhaseEnded for Game={GameCode}, Round={Round}, ExecuteAt={ExecuteAt}",
            gameCode, roundNumber, task.ExecuteAt);
        return task.Id;
    }

    public Guid ScheduleScorePhaseEnded(string gameCode, int roundNumber, Guid memeId, int delaySeconds = DefaultDelaySeconds)
    {
        ValidateDelay(ref delaySeconds);
        var task = new ScheduledGameTask(
            Guid.NewGuid(),
            GameTaskType.ScorePhaseEnded,
            gameCode,
            roundNumber,
            DateTimeOffset.UtcNow.AddSeconds(delaySeconds),
            memeId);

        ScheduleTask(task);
        _logger.LogInformation(
            "Scheduled HasRoundEnded for Game={GameCode}, Round={Round}, Meme={SubmissionId}, ExecuteAt={ExecuteAt}",
            gameCode, roundNumber, memeId, task.ExecuteAt);
        return task.Id;
    }

    public Guid ScheduleStartNewRound(string gameCode, int roundNumber, int delaySeconds = DefaultDelaySeconds)
    {
        ValidateDelay(ref delaySeconds);
        var task = new ScheduledGameTask(
            Guid.NewGuid(),
            GameTaskType.StartNewRound,
            gameCode,
            roundNumber,
            DateTimeOffset.UtcNow.AddSeconds(delaySeconds));

        ScheduleTask(task);
        _logger.LogInformation(
            "Scheduled StartNewRound for Game={GameCode}, NextRound={Round}, ExecuteAt={ExecuteAt}",
            gameCode, roundNumber, task.ExecuteAt);
        return task.Id;
    }

    public bool CancelTask(Guid taskId)
    {
        lock (_lock)
        {
            if (!_taskLookup.TryGetValue(taskId, out var task))
            {
                return false;
            }

            _taskHeap.Remove(task);
            _taskLookup.Remove(taskId);
            _logger.LogInformation("Cancelled task {TaskId}", taskId);

            ArmTimerForNextTask();
            return true;
        }
    }

    public int CancelAllTasksForGame(string gameCode)
    {
        lock (_lock)
        {
            var tasksToRemove = _taskLookup.Values
                .Where(t => t.GameCode == gameCode)
                .ToList();

            foreach (var task in tasksToRemove)
            {
                _taskHeap.Remove(task);
                _taskLookup.Remove(task.Id);
            }

            _logger.LogInformation("Cancelled {Count} tasks for game {GameCode}", tasksToRemove.Count, gameCode);
            ArmTimerForNextTask();
            return tasksToRemove.Count;
        }
    }

    internal void ProcessDueTasks()
    {
        List<ScheduledGameTask> dueTasks;
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;
            dueTasks = new List<ScheduledGameTask>();

            while (_taskHeap.Count > 0)
            {
                var next = _taskHeap.Min;
                if (next == null || next.ExecuteAt > now)
                {
                    break;
                }

                _taskHeap.Remove(next);
                _taskLookup.Remove(next.Id);
                dueTasks.Add(next);
            }

            ArmTimerForNextTask();
        }

        foreach (var task in dueTasks)
        {
            try
            {
                _logger.LogInformation(
                    "Executing task {TaskId} ({TaskType}) for Game={GameCode}, Round={Round}",
                    task.Id, task.TaskType, task.GameCode, task.RoundNumber);

                TaskDue?.Invoke(this, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing task {TaskId}", task.Id);
            }
        }
    }

    private void ScheduleTask(ScheduledGameTask task)
    {
        lock (_lock)
        {
            _taskHeap.Add(task);
            _taskLookup[task.Id] = task;
            ArmTimerForNextTask();
        }
    }

    private void ArmTimerForNextTask()
    {
        _timer?.Dispose();
        _timer = null;

        if (_taskHeap.Count == 0)
        {
            _logger.LogDebug("No tasks scheduled, timer disarmed");
            return;
        }

        var next = _taskHeap.Min;
        if (next == null)
        {
            return;
        }

        var delay = next.ExecuteAt - DateTimeOffset.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        _logger.LogDebug("Arming timer for {Delay}ms (next task at {ExecuteAt})", delay.TotalMilliseconds, next.ExecuteAt);
        _timer = new Timer(_ => ProcessDueTasks(), null, delay, Timeout.InfiniteTimeSpan);
    }

    private static void ValidateDelay(ref int delaySeconds)
    {
        if (delaySeconds < MinDelaySeconds)
        {
            delaySeconds = MinDelaySeconds;
        }
        else if (delaySeconds > MaxDelaySeconds)
        {
            delaySeconds = MaxDelaySeconds;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timer?.Dispose();
        _disposed = true;
    }
}
