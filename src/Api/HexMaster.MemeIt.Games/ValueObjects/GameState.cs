using Orleans;

namespace HexMaster.MemeIt.Games.ValueObjects;

[GenerateSerializer]
public class GameState
{
    [Id(0)] public required string GameCode { get; set; }
    [Id(1)] public required string Status { get; set; } = GameStatus.Uninitialized.Id;
    [Id(2)] public required List<(string Id, string Name)> Players { get; set; }
    [Id(3)] public string? Password { get; set; }
    [Id(4)] public string? LeaderId { get; set; } // PlayerId of the leader
    [Id(5)] public GameSettings Settings { get; set; } = new(); // Game settings (max players, rounds, category)
    [Id(6)] public Dictionary<string, bool> PlayerReadyStates { get; set; } = new(); // PlayerID -> Ready status
    [Id(7)] public Dictionary<string, PlayerMemeAssignment> PlayerMemeAssignments { get; set; } = new(); // PlayerID -> Meme Template Assignment
    [Id(8)] public int CurrentRound { get; set; } = 1; // Current round number (1-based)
}

[GenerateSerializer]
public class PlayerMemeAssignment
{
    [Id(0)] public required string MemeTemplateId { get; set; }
    [Id(1)] public required string MemeTemplateName { get; set; }
    [Id(2)] public required string MemeTemplateImageUrl { get; set; }
    [Id(3)] public required int CurrentRound { get; set; }
    [Id(4)] public DateTime AssignedAt { get; set; }
}
