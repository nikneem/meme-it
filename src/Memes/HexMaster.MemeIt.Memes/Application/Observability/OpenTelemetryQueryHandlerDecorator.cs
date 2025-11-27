using System.Diagnostics;
using HexMaster.MemeIt.Memes.Abstractions.Application;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;

namespace HexMaster.MemeIt.Memes.Application.Observability;

/// <summary>
/// Decorator that adds OpenTelemetry instrumentation to query handlers
/// </summary>
public sealed class OpenTelemetryQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly MemesMetrics _metrics;

    public OpenTelemetryQueryHandlerDecorator(
        IQueryHandler<TQuery, TResult> inner,
        MemesMetrics metrics)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
    }

    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        var queryName = typeof(TQuery).Name.Replace("Query", "");

        using var activity = MemesActivitySource.Instance.StartActivity(
            queryName,
            ActivityKind.Internal);

        activity?.SetTag("query.type", typeof(TQuery).Name);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _inner.HandleAsync(query, cancellationToken).ConfigureAwait(false);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordHandlerDuration(queryName, stopwatch.Elapsed.TotalMilliseconds, success: true);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _metrics.RecordHandlerDuration(queryName, stopwatch.Elapsed.TotalMilliseconds, success: false);
            _metrics.RecordCommandFailed(queryName, ex.GetType().Name);
            throw;
        }
    }
}
