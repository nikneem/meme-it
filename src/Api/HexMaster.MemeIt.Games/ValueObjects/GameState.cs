namespace HexMaster.MemeIt.Games.ValueObjects;

public class GameState
{
    public required string GameCode { get; set; }
    public required string Status { get; set; } = GameStatus.Uninitialized.Id;
    public required List<(string Id, string Name)> Players { get; set; }
    public string? Password { get; set; }
    public string? LeaderId { get; set; } // PlayerId of the leader
    public GameSettings Settings { get; set; } = new(); // Game settings (max players, rounds, category)
    public Dictionary<string, bool> PlayerReadyStates { get; set; } = new(); // PlayerID -> Ready status
    public Dictionary<string, PlayerMemeAssignment> PlayerMemeAssignments { get; set; } = new(); // PlayerID -> Meme Template Assignment
    public int CurrentRound { get; set; } = 1; // Current round number (1-based)
}

public class PlayerMemeAssignment
{
    public required string MemeTemplateId { get; set; }
    public required string MemeTemplateName { get; set; }
    public required string MemeTemplateImageUrl { get; set; }
    public required int CurrentRound { get; set; }
    public DateTime AssignedAt { get; set; }
}
