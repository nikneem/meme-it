using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.ValueObjects;
using Orleans;

namespace HexMaster.MemeIt.Games.Abstractions.Grains;

public interface IGameGrain : IGrainWithStringKey
{
    Task<GameState> GetCurrent();
    Task<GameState> CreateGame(CreateGameCommand initialState);
    Task<GameState> JoinGame(JoinGameState playerState);
}