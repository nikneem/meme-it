using HexMaster.MemeIt.Games.ValueObjects;
using Orleans;

namespace HexMaster.MemeIt.Games.Abstractions.Grains;

public interface IGameGainObserver : IGrainObserver
{
    void OnGameUpdated(GameState state);
}