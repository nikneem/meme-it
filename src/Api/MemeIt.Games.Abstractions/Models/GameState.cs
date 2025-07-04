using Orleans;

namespace MemeIt.Games.Abstractions.Models;

[GenerateSerializer]
public class GameState
{
    [Id(0)]
    public string Id { get; set; } = string.Empty;

    [Id(1)]
    public string Name { get; set; } = string.Empty;

    [Id(2)]
    public string GameCode { get; set; } = string.Empty;

    [Id(3)]
    public string GameMasterId { get; set; } = string.Empty;

    [Id(4)]
    public GameStatus Status { get; set; } = GameStatus.Lobby;

    [Id(5)]
    public List<string> PlayerIds { get; set; } = new();

    [Id(6)]
    public int CurrentRound { get; set; } = 0;

    [Id(7)]
    public int MaxRounds { get; set; } = 5;

    [Id(8)]
    public int RoundDurationMinutes { get; set; } = 1;

    [Id(9)]
    public Dictionary<int, RoundState> Rounds { get; set; } = new();

    [Id(10)]
    public Dictionary<string, int> PlayerScores { get; set; } = new();

    [Id(11)]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Id(12)]
    public DateTimeOffset? StartedAt { get; set; }

    [Id(13)]
    public DateTimeOffset? FinishedAt { get; set; }

    [Id(14)]
    public GameOptions Options { get; set; } = new();
}

[GenerateSerializer]
public class RoundState
{
    [Id(0)]
    public int RoundNumber { get; set; }

    [Id(1)]
    public RoundStatus Status { get; set; } = RoundStatus.TextEntry;

    [Id(2)]
    public Dictionary<string, PlayerMeme> PlayerMemes { get; set; } = new();

    [Id(3)]
    public Dictionary<string, Dictionary<string, int>> Scores { get; set; } = new();

    [Id(4)]
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    [Id(5)]
    public DateTimeOffset? TextEntryDeadline { get; set; }

    [Id(6)]
    public DateTimeOffset? ScoringDeadline { get; set; }

    [Id(7)]
    public DateTimeOffset? FinishedAt { get; set; }
}

[GenerateSerializer]
public class PlayerMeme
{
    [Id(0)]
    public string PlayerId { get; set; } = string.Empty;

    [Id(1)]
    public string MemeId { get; set; } = string.Empty;

    [Id(2)]
    public Dictionary<string, string> TextEntries { get; set; } = new();

    [Id(3)]
    public bool IsSubmitted { get; set; } = false;

    [Id(4)]
    public DateTimeOffset? SubmittedAt { get; set; }

    [Id(5)]
    public int TotalScore { get; set; } = 0;

    [Id(6)]
    public int ScoreCount { get; set; } = 0;
}

[GenerateSerializer]
public class GameOptions
{
    [Id(0)]
    public int MaxPlayers { get; set; } = 8;

    [Id(1)]
    public int MinPlayers { get; set; } = 2;

    [Id(2)]
    public bool AllowSpectators { get; set; } = true;

    [Id(3)]
    public bool IsPrivate { get; set; } = false;

    [Id(4)]
    public string? Password { get; set; }

    [Id(5)]
    public List<string> AllowedCategories { get; set; } = new();
}

public enum GameStatus
{
    Lobby,
    Started,
    Finished,
    Cancelled
}

public enum RoundStatus
{
    TextEntry,
    Scoring,
    Finished
}
