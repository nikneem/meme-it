using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Features;

public record GameSettingsResponse(int MaxPlayers, int NumberOfRounds, string Category);

public record GameDetailsResponse(string GameCode, List<string> Players, bool IsPasswordProtected, GameSettingsResponse Settings)
{
    public static GameDetailsResponse FromGameState(GameState gameState)
    {
        return new GameDetailsResponse(
            gameState.GameCode,
            gameState.Players.Select(p => p.Name).ToList(),
            !string.IsNullOrWhiteSpace(gameState.Password),
            new GameSettingsResponse(
                gameState.Settings.MaxPlayers,
                gameState.Settings.NumberOfRounds,
                gameState.Settings.Category)
            );
    }
}