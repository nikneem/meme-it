using Orleans;

namespace MemeIt.Games.Abstractions.Models;

[GenerateSerializer]
public class PlayerState
{
    [Id(0)]
    public string Id { get; set; } = string.Empty;

    [Id(1)]
    public string Name { get; set; } = string.Empty;

    [Id(2)]
    public string? CurrentGameId { get; set; }

    [Id(3)]
    public bool IsGameMaster { get; set; } = false;

    [Id(4)]
    public PlayerStatus Status { get; set; } = PlayerStatus.InLobby;

    [Id(5)]
    public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.UtcNow;

    [Id(6)]
    public int TotalScore { get; set; } = 0;

    [Id(7)]
    public List<string> GameHistory { get; set; } = new();
}

public enum PlayerStatus
{
    InLobby,
    Playing,
    Scoring,
    Disconnected
}
