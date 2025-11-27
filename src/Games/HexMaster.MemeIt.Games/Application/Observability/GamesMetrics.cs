using System.Diagnostics.Metrics;

namespace HexMaster.MemeIt.Games.Application.Observability;

public sealed class GamesMetrics
{
    private readonly Counter<long> _gamesCreated;
    private readonly Counter<long> _playersJoined;
    private readonly Counter<long> _gamesStarted;
    private readonly Counter<long> _memesSubmitted;
    private readonly Counter<long> _memesRated;
    private readonly Counter<long> _roundsCompleted;
    private readonly Histogram<double> _handlerDuration;
    private readonly Counter<long> _commandsFailed;

    public GamesMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("HexMaster.MemeIt.Games", "1.0.0");

        _gamesCreated = meter.CreateCounter<long>(
            name: "games.created",
            unit: "{game}",
            description: "Total number of games created");

        _playersJoined = meter.CreateCounter<long>(
            name: "games.players.joined",
            unit: "{player}",
            description: "Total number of players joined");

        _gamesStarted = meter.CreateCounter<long>(
            name: "games.started",
            unit: "{game}",
            description: "Total number of games started");

        _memesSubmitted = meter.CreateCounter<long>(
            name: "games.memes.submitted",
            unit: "{meme}",
            description: "Total number of memes submitted");

        _memesRated = meter.CreateCounter<long>(
            name: "games.memes.rated",
            unit: "{rating}",
            description: "Total number of meme ratings");

        _roundsCompleted = meter.CreateCounter<long>(
            name: "games.rounds.completed",
            unit: "{round}",
            description: "Total number of rounds completed");

        _handlerDuration = meter.CreateHistogram<double>(
            name: "games.handler.duration",
            unit: "ms",
            description: "Handler execution duration");

        _commandsFailed = meter.CreateCounter<long>(
            name: "games.commands.failed",
            unit: "{command}",
            description: "Total number of failed commands");
    }

    public void RecordGameCreated() => _gamesCreated.Add(1);

    public void RecordPlayerJoined() => _playersJoined.Add(1);

    public void RecordGameStarted(int playerCount)
    {
        _gamesStarted.Add(1, new KeyValuePair<string, object?>("player.count", playerCount));
    }

    public void RecordMemeSubmitted() => _memesSubmitted.Add(1);

    public void RecordMemeRated(int rating)
    {
        _memesRated.Add(1, new KeyValuePair<string, object?>("rating.value", rating));
    }

    public void RecordRoundCompleted(int roundNumber)
    {
        _roundsCompleted.Add(1, new KeyValuePair<string, object?>("round.number", roundNumber));
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
