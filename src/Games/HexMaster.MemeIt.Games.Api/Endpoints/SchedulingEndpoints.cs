using HexMaster.MemeIt.Games.Abstractions.Services;

namespace HexMaster.MemeIt.Games.Api.Endpoints;

public static class SchedulingEndpoints
{
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/games/{gameCode}/scheduling")
            .WithTags("Scheduling");

        group.MapPost("/creative-phase", (
            string gameCode,
            int roundNumber,
            int? delaySeconds,
            IScheduledTaskService schedulingService) =>
        {
            var taskId = schedulingService.ScheduleCreativePhaseEnded(
                gameCode,
                roundNumber,
                delaySeconds ?? 30);

            return Results.Ok(new { taskId, message = "Creative phase task scheduled" });
        })
        .WithName("ScheduleCreativePhaseEnded")
        .WithSummary("Schedule a creative phase ended task");

        group.MapPost("/score-phase", (
            string gameCode,
            int roundNumber,
            Guid memeId,
            int? delaySeconds,
            IScheduledTaskService schedulingService) =>
        {
            var taskId = schedulingService.ScheduleScorePhaseEnded(
                gameCode,
                roundNumber,
                memeId,
                delaySeconds ?? 30);

            return Results.Ok(new { taskId, message = "Score phase task scheduled" });
        })
        .WithName("ScheduleScorePhaseEnded")
        .WithSummary("Schedule a score phase ended task");

        group.MapDelete("/tasks/{taskId}", (
            Guid taskId,
            IScheduledTaskService schedulingService) =>
        {
            var cancelled = schedulingService.CancelTask(taskId);
            return cancelled
                ? Results.Ok(new { message = "Task cancelled" })
                : Results.NotFound(new { message = "Task not found" });
        })
        .WithName("CancelScheduledTask")
        .WithSummary("Cancel a scheduled task");

        group.MapDelete("/all", (
            string gameCode,
            IScheduledTaskService schedulingService) =>
        {
            var cancelledCount = schedulingService.CancelAllTasksForGame(gameCode);
            return Results.Ok(new { cancelledCount, message = $"{cancelledCount} tasks cancelled" });
        })
        .WithName("CancelAllTasksForGame")
        .WithSummary("Cancel all scheduled tasks for a game");

        return app;
    }
}
