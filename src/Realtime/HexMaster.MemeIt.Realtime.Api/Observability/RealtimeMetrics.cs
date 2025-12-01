using System.Diagnostics.Metrics;

namespace HexMaster.MemeIt.Realtime.Api.Observability;

public sealed class RealtimeMetrics
{
    private readonly Counter<long> _messagesPublished;
    private readonly Counter<long> _connectionEstablished;
    private readonly Counter<long> _connectionClosed;
    private readonly Histogram<double> _messageLatency;
    private readonly Counter<long> _publishFailed;

    public RealtimeMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("HexMaster.MemeIt.Realtime", "1.0.0");

        _messagesPublished = meter.CreateCounter<long>(
            name: "realtime.messages.published",
            unit: "{message}",
            description: "Total number of messages published to SignalR clients");

        _connectionEstablished = meter.CreateCounter<long>(
            name: "realtime.connections.established",
            unit: "{connection}",
            description: "Total number of SignalR connections established");

        _connectionClosed = meter.CreateCounter<long>(
            name: "realtime.connections.closed",
            unit: "{connection}",
            description: "Total number of SignalR connections closed");

        _messageLatency = meter.CreateHistogram<double>(
            name: "realtime.message.latency",
            unit: "ms",
            description: "Message publish latency in milliseconds");

        _publishFailed = meter.CreateCounter<long>(
            name: "realtime.publish.failed",
            unit: "{failure}",
            description: "Total number of failed message publishes");
    }

    public void RecordMessagePublished(string messageType)
    {
        _messagesPublished.Add(1, new KeyValuePair<string, object?>("message.type", messageType));
    }

    public void RecordConnectionEstablished(string? connectionId = null)
    {
        _connectionEstablished.Add(1, new KeyValuePair<string, object?>("connection.id", connectionId ?? "unknown"));
    }

    public void RecordConnectionClosed(string? connectionId = null)
    {
        _connectionClosed.Add(1, new KeyValuePair<string, object?>("connection.id", connectionId ?? "unknown"));
    }

    public void RecordMessageLatency(double latencyMs, string messageType)
    {
        _messageLatency.Record(latencyMs, new KeyValuePair<string, object?>("message.type", messageType));
    }

    public void RecordPublishFailed(string messageType, string? error = null)
    {
        _publishFailed.Add(1,
            new KeyValuePair<string, object?>("message.type", messageType),
            new KeyValuePair<string, object?>("error", error ?? "unknown"));
    }
}
