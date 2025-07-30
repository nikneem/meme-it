using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Features;

public record GameSettingsResponse(int MaxPlayers, int NumberOfRounds, string Category);

public record GameDetailsResponse(string GameCode, string Status, List<string> Players, string PlayerId, bool IsPasswordProtected, GameSettingsResponse Settings)
{
    public static GameDetailsResponse FromGameState(GameState gameState, string playerId)
    {
        return new GameDetailsResponse(
            gameState.GameCode,
            gameState.Status,
            gameState.Players.Select(p => p.Name).ToList(),
            playerId,
            !string.IsNullOrWhiteSpace(gameState.Password),
            new GameSettingsResponse(
                gameState.Settings.MaxPlayers,
                gameState.Settings.NumberOfRounds,
                gameState.Settings.Category)
            );
    }
}