namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Represents the lifecycle stages a game can be in.
/// </summary>
public enum GameState
{
    Lobby = 0,
    InProgress = 1,
    Scoring = 2,
    Completed = 3
}
