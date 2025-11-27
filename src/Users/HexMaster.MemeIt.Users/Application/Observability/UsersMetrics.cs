using System.Diagnostics.Metrics;

namespace HexMaster.MemeIt.Users.Application.Observability;

public sealed class UsersMetrics
{
    private readonly Counter<long> _usersJoined;
    private readonly Histogram<double> _handlerDuration;
    private readonly Counter<long> _commandsFailed;

    public UsersMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("HexMaster.MemeIt.Users", "1.0.0");

        _usersJoined = meter.CreateCounter<long>(
            name: "users.joined",
            unit: "{user}",
            description: "Total number of users joined");

        _handlerDuration = meter.CreateHistogram<double>(
            name: "users.handler.duration",
            unit: "ms",
            description: "Handler execution duration");

        _commandsFailed = meter.CreateCounter<long>(
            name: "users.commands.failed",
            unit: "{command}",
            description: "Total number of failed commands");
    }

    public void RecordUserJoined() => _usersJoined.Add(1);

    public void RecordHandlerDuration(string commandName, double durationMs, bool success)
    {
        _handlerDuration.Record(durationMs,
            new KeyValuePair<string, object?>("command.name", commandName),
            new KeyValuePair<string, object?>("success", success));
    }

    public void RecordCommandFailed(string commandName, string errorType)
    {
        _commandsFailed.Add(1,
            new KeyValuePair<string, object?>("command.name", commandName),
            new KeyValuePair<string, object?>("error.type", errorType));
    }
}
