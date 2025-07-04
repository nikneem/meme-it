namespace HexMaster.MemeIt.Games.Abstractions.DomainModel;

public interface IGameState
{

    Guid Id { get; }
    string Name { get; }


    IReadOnlyList<IGamePlayer> Players { get; }

    IGameOptions Options { get; }

    void AddPlayer(IGamePlayer player);
    bool RemovePlayer(IGamePlayer player);

}