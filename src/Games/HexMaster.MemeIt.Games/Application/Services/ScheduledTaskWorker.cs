using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Games.EndCreativePhase;
using HexMaster.MemeIt.Games.Application.Games.EndScorePhase;
using HexMaster.MemeIt.Games.Application.Games.StartNewRound;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Services;

public sealed class ScheduledTaskWorker : BackgroundService
{
    private readonly ScheduledTaskService _taskService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledTaskWorker> _logger;

    public ScheduledTaskWorker(
        ScheduledTaskService taskService,
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledTaskWorker> logger)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
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
                    _ = HandleCreativePhaseEndedAsync(task);
                    break;

                case GameTaskType.ScorePhaseEnded:
                    _ = HandleScorePhaseEndedAsync(task);
                    break;

                case GameTaskType.StartNewRound:
                    _ = HandleStartNewRoundAsync(task);
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

    private async Task HandleCreativePhaseEndedAsync(ScheduledGameTask task)
    {
        _logger.LogInformation(
            "Creative phase ended for Game={GameCode}, Round={Round}",
            task.GameCode, task.RoundNumber);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>>();

            var command = new EndCreativePhaseCommand(task.GameCode, task.RoundNumber);
            await handler.HandleAsync(command);

            _logger.LogInformation(
                "Successfully ended creative phase for Game={GameCode}, Round={Round}",
                task.GameCode, task.RoundNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to end creative phase for Game={GameCode}, Round={Round}",
                task.GameCode, task.RoundNumber);
        }
    }

    private async Task HandleScorePhaseEndedAsync(ScheduledGameTask task)
    {
        _logger.LogInformation(
            "Score phase ended for Game={GameCode}, Round={Round}, Meme={SubmissionId}",
            task.GameCode, task.RoundNumber, task.MemeId);

        if (!task.MemeId.HasValue)
        {
            _logger.LogError(
                "Cannot end score phase: SubmissionId is missing for Game={GameCode}, Round={Round}",
                task.GameCode, task.RoundNumber);
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult>>();

            var command = new EndScorePhaseCommand(task.GameCode, task.RoundNumber, task.MemeId.Value);
            var result = await handler.HandleAsync(command);

            if (result.RoundComplete)
            {
                _logger.LogInformation(
                    "All memes scored. Round {Round} complete for Game={GameCode}",
                    task.RoundNumber, task.GameCode);
            }
            else
            {
                _logger.LogInformation(
                    "Started scoring next meme in Game={GameCode}, Round={Round}",
                    task.GameCode, task.RoundNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to end score phase for Game={GameCode}, Round={Round}, Meme={SubmissionId}",
                task.GameCode, task.RoundNumber, task.MemeId);
        }
    }

    private async Task HandleStartNewRoundAsync(ScheduledGameTask task)
    {
        _logger.LogInformation(
            "Starting new round for Game={GameCode}, Round={Round}",
            task.GameCode, task.RoundNumber);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<StartNewRoundCommand, StartNewRoundResult>>();

            var command = new StartNewRoundCommand(task.GameCode, task.RoundNumber);
            var result = await handler.HandleAsync(command);

            _logger.LogInformation(
                "Successfully started round {Round} for game {GameCode}",
                result.RoundNumber, result.GameCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to start new round for Game={GameCode}, Round={Round}",
                task.GameCode, task.RoundNumber);
        }
    }
}
