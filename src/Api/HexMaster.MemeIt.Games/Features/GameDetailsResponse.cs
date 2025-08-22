using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Features;

public record GameSettingsResponse(int MaxPlayers, int NumberOfRounds, string Category);

public record PlayerResponse(string Id, string Name, bool IsReady);

public record GameDetailsResponse(string GameCode, string Status, List<PlayerResponse> Players, string PlayerId, bool IsPasswordProtected, GameSettingsResponse Settings)
{
    public static GameDetailsResponse FromGameState(GameState gameState, string playerId)
    {
        var players = gameState.Players.Select(p => new PlayerResponse(
            p.Id, 
            p.Name, 
            gameState.PlayerReadyStates.GetValueOrDefault(p.Id, false)
        )).ToList();

        return new GameDetailsResponse(
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

// Broadcast-specific response without player ID for WebPubSub messages
public record GameStateBroadcastResponse(string GameCode, string Status, List<PlayerResponse> Players, bool IsPasswordProtected, GameSettingsResponse Settings)
{
    public static GameStateBroadcastResponse FromGameState(GameState gameState)
    {
        var players = gameState.Players.Select(p => new PlayerResponse(
            p.Id, 
            p.Name, 
            gameState.PlayerReadyStates.GetValueOrDefault(p.Id, false)
        )).ToList();

        return new GameStateBroadcastResponse(
            gameState.GameCode,
            gameState.Status,
            players,
            !string.IsNullOrWhiteSpace(gameState.Password),
            new GameSettingsResponse(
                gameState.Settings.MaxPlayers,
                gameState.Settings.NumberOfRounds,
                gameState.Settings.Category)
            );
    }
}