using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Features.CreateGame;

public record CreateGameResponse(string GameCode, string Status, List<PlayerResponse> Players, string PlayerId, bool IsPasswordProtected, GameSettingsResponse Settings)
{
    public static CreateGameResponse FromGameState(GameState gameState, string playerId)
    {
        var players = gameState.Players.Select(p => new PlayerResponse(
            p.Id, 
            p.Name, 
            gameState.PlayerReadyStates.GetValueOrDefault(p.Id, false)
        )).ToList();

        return new CreateGameResponse(
            gameState.GameCode,
            gameState.Status,
            players,
            playerId,
            !string.IsNullOrWhiteSpace(gameState.Password),
            new GameSettingsResponse(
                gameState.Settings.MaxPlayers,
                gameState.Settings.NumberOfRounds,
                gameState.Settings.Category)
            );
    }
}