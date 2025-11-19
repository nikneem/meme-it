namespace HexMaster.MemeIt.Games.ValueObjects;

public class GameSettings
{
    public int MaxPlayers { get; set; } = 10;
    public int NumberOfRounds { get; set; } = 5;
    public string Category { get; set; } = "All";
}
