using System.Diagnostics.Metrics;

namespace HexMaster.MemeIt.Memes.Application.Observability;

public sealed class MemesMetrics
{
    private readonly Counter<long> _templatesCreated;
    private readonly Counter<long> _templatesUpdated;
    private readonly Counter<long> _templatesDeleted;
    private readonly Counter<long> _templatesRetrieved;
    private readonly Histogram<double> _handlerDuration;
    private readonly Counter<long> _commandsFailed;

    public MemesMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("HexMaster.MemeIt.Memes", "1.0.0");

        _templatesCreated = meter.CreateCounter<long>(
            name: "meme_templates.created",
            unit: "{template}",
            description: "Total number of meme templates created");

        _templatesUpdated = meter.CreateCounter<long>(
            name: "meme_templates.updated",
            unit: "{template}",
            description: "Total number of meme templates updated");

        _templatesDeleted = meter.CreateCounter<long>(
            name: "meme_templates.deleted",
            unit: "{template}",
            description: "Total number of meme templates deleted");

        _templatesRetrieved = meter.CreateCounter<long>(
            name: "meme_templates.retrieved",
            unit: "{template}",
            description: "Total number of meme templates retrieved");

        _handlerDuration = meter.CreateHistogram<double>(
            name: "memes.handler.duration",
            unit: "ms",
            description: "Handler execution duration");

        _commandsFailed = meter.CreateCounter<long>(
            name: "memes.commands.failed",
            unit: "{command}",
            description: "Total number of failed commands");
    }

    public void RecordTemplateCreated(int textFieldCount)
    {
        _templatesCreated.Add(1, new KeyValuePair<string, object?>("text_fields.count", textFieldCount));
    }

    public void RecordTemplateUpdated() => _templatesUpdated.Add(1);

    public void RecordTemplateDeleted() => _templatesDeleted.Add(1);

    public void RecordTemplateRetrieved(string operation)
    {
        _templatesRetrieved.Add(1, new KeyValuePair<string, object?>("operation", operation));
    }

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
