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
    Task<GameState> UpdateSettings(string playerId, GameSettings settings);
    Task<GameState> StartGame(string playerId);
    Task<GameState> SetPlayerReadyStatus(string playerId, bool isReady);
}