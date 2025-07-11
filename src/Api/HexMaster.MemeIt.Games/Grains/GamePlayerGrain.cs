﻿using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.ValueObjects;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Utilities;

namespace HexMaster.MemeIt.Games.Grains;

public class GamePlayerGrain([PersistentState(stateName: "gamePlayerState", storageName: "games")] IPersistentState<GamePlayerState> state,
    ILogger<ObserverManager<IGamePlayerGrainObserver>> gameLogger) : Grain, IGamePlayerGrain
{
    public async Task<GamePlayerState> CreatePlayer(GamePlayerState initialState)
    {
        // Set the state and persist it
        state.State = initialState;
        await state.WriteStateAsync();
        return initialState;
    }
}