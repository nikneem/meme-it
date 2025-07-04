using HexMaster.MemeIt.Games.Abstractions.DomainModel;
using Orleans;

namespace HexMaster.MemeIt.Games.Abstractions.Grains;

public interface IGameGrain : IGrainWithStringKey
{
    ValueTask<IGamePlayer> AddPlayer(IGamePlayer player);
    ValueTask<bool> RemovePlayer(IGamePlayer player);
}

