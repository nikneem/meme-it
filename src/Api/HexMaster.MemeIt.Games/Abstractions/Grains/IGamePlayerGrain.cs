﻿using HexMaster.MemeIt.Games.ValueObjects;
using Orleans;

namespace HexMaster.MemeIt.Games.Abstractions.Grains;

public interface IGamePlayerGrain : IGrainWithStringKey
{
    Task<GamePlayerState> CreatePlayer(GamePlayerState initialState);

}