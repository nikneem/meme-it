namespace HexMaster.MemeIt.Games.ValueObjects;

public class GamePlayerState
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public bool IsReady { get; set; } = false;
}
