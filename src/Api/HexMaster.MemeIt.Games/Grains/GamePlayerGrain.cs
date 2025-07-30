using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.ValueObjects;
using Orleans;
using Orleans.Runtime;

namespace HexMaster.MemeIt.Games.Grains;

public class GamePlayerGrain([PersistentState(stateName: "gamePlayerState", storageName: "games")] IPersistentState<GamePlayerState> state) : Grain, IGamePlayerGrain
{
    public async Task<GamePlayerState> CreatePlayer(GamePlayerState initialState)
    {
        // Set the state and persist it
        state.State = initialState;
        await state.WriteStateAsync();
        return initialState;
    }

    public async Task<GamePlayerState> SetReadyStatus(bool isReady)
    {
        state.State.IsReady = isReady;
        await state.WriteStateAsync();
        return state.State;
    }

    public Task<GamePlayerState> GetCurrentState()
    {
        return Task.FromResult(state.State);
    }
}