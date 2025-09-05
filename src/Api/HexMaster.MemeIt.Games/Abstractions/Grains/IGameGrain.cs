using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
using HexMaster.MemeIt.Games.ValueObjects;
using Orleans;

namespace HexMaster.MemeIt.Games.Abstractions.Grains;

public interface IGameGrain : IGrainWithStringKey
{
    Task<GameState> GetCurrent();
    Task<GameState> CreateGame(CreateGameCommand initialState);
    Task<GameState> JoinGame(JoinGameCommand playerState);
    Task<GameState> LeaveGame(string playerId);
    Task<GameState> KickPlayer(string hostPlayerId, string targetPlayerId);
    Task<GameState> UpdateSettings(string playerId, GameSettings settings);
    Task<GameState> StartGame(string playerId);
    Task<GameState> SetPlayerReadyStatus(string playerId, bool isReady);
    Task<PlayerMemeAssignment?> GetPlayerMemeAssignment(string playerId);
    Task<GameState> AssignMemeToPlayer(string playerId, string memeTemplateId, string memeTemplateName, string memeTemplateImageUrl);
}