using System.Diagnostics;
using HexMaster.MemeIt.Users.Abstractions.Application;
using HexMaster.MemeIt.Users.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Users.Application.Observability;

/// <summary>
/// Decorator that adds OpenTelemetry instrumentation to command handlers
/// </summary>
public sealed class OpenTelemetryCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly UsersMetrics _metrics;

    public OpenTelemetryCommandHandlerDecorator(
        ICommandHandler<TCommand, TResult> inner,
        UsersMetrics metrics)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var commandName = typeof(TCommand).Name.Replace("Command", "");

        using var activity = UsersActivitySource.Instance.StartActivity(
            commandName,
            ActivityKind.Internal);

        activity?.SetTag("command.type", typeof(TCommand).Name);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _inner.HandleAsync(command, cancellationToken).ConfigureAwait(false);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordHandlerDuration(commandName, stopwatch.Elapsed.TotalMilliseconds, success: true);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _metrics.RecordHandlerDuration(commandName, stopwatch.Elapsed.TotalMilliseconds, success: false);
            _metrics.RecordCommandFailed(commandName, ex.GetType().Name);
            throw;
        }
    }
}
