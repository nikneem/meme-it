using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Features;

public record GameDetailsResponse(string GameCode, List<string> Players, bool IsPasswordProtected)
{
    public static GameDetailsResponse FromGameState(GameState gameState)
    {
        return new GameDetailsResponse(
            gameState.GameCode,
            gameState.Players.Select(p => p.Name).ToList(),
            !string.IsNullOrWhiteSpace(gameState.Password));
    }
}