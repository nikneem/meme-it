using HexMaster.MemeIt.Games.Abstractions.DomainModel;
using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.DomainModels;
using Orleans.Runtime;

namespace HexMaster.MemeIt.Games.Grains;

public sealed class GameGrain([PersistentState("count")] IPersistentState<GameState> game) : IGameGrain
{
    public async ValueTask<IGamePlayer> AddPlayer(IGamePlayer player)
    {
        game.State.AddPlayer(player);
        await game.WriteStateAsync();
        return player;
    }

    public async ValueTask<bool> RemovePlayer(IGamePlayer player)
    {
        var removeResult = game.State.RemovePlayer(player);
        await game.WriteStateAsync();
        return removeResult;

    }
}