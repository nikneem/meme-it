using HexMaster.MemeIt.Games.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Services;

public sealed class ScheduledTaskWorker : BackgroundService
{
    private readonly ScheduledTaskService _taskService;
    private readonly ILogger<ScheduledTaskWorker> _logger;

    public ScheduledTaskWorker(
        ScheduledTaskService taskService,
        ILogger<ScheduledTaskWorker> logger)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _taskService.TaskDue += OnTaskDue;
        _logger.LogInformation("ScheduledTaskWorker started");

        stoppingToken.Register(() =>
        {
            _taskService.TaskDue -= OnTaskDue;
            _logger.LogInformation("ScheduledTaskWorker stopping");
        });

        return Task.CompletedTask;
    }

    private void OnTaskDue(object? sender, ScheduledGameTask task)
    {
        try
        {
            switch (task.TaskType)
            {
                case GameTaskType.CreativePhaseEnded:
                    HandleCreativePhaseEnded(task);
                    break;

                case GameTaskType.ScorePhaseEnded:
                    HandleScorePhaseEnded(task);
                    break;

                case GameTaskType.RoundEnded:
                    HandleRoundEnded(task);
                    break;

                default:
                    _logger.LogWarning("Unknown task type {TaskType} for task {TaskId}", task.TaskType, task.Id);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling task {TaskId} of type {TaskType}", task.Id, task.TaskType);
        }
    }

    private void HandleCreativePhaseEnded(ScheduledGameTask task)
    {
        _logger.LogInformation(
            "Creative phase ended for Game={GameCode}, Round={Round}",
            task.GameCode, task.RoundNumber);

        // TODO: Publish CreativePhaseEndedIntegrationEvent or send EndCreativePhaseCommand
        // Example:
        // await _mediator.Send(new EndCreativePhaseCommand(task.GameCode, task.RoundNumber));
    }

    private void HandleScorePhaseEnded(ScheduledGameTask task)
    {
        _logger.LogInformation(
            "Score phase ended for Game={GameCode}, Round={Round}, Meme={MemeId}",
            task.GameCode, task.RoundNumber, task.MemeId);

        // TODO: Publish ScorePhaseEndedIntegrationEvent or send EndScorePhaseCommand
        // Example:
        // await _mediator.Send(new EndScorePhaseCommand(task.GameCode, task.RoundNumber, task.MemeId!.Value));
    }

    private void HandleRoundEnded(ScheduledGameTask task)
    {
        _logger.LogInformation(
            "Round ended for Game={GameCode}, Round={Round}",
            task.GameCode, task.RoundNumber);

        // TODO: Publish RoundEndedIntegrationEvent or send EndRoundCommand
        // Example:
        // await _mediator.Send(new EndRoundCommand(task.GameCode, task.RoundNumber));
    }
}
