using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Features.CreateGame;

public record CreateGameResponse(string GameCode, string Status, List<string> Players, string PlayerId, bool IsPasswordProtected, GameSettingsResponse Settings)
{
    public static CreateGameResponse FromGameState(GameState gameState, string playerId)
    {
        return new CreateGameResponse(
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