using HexMaster.MemeIt.Games.Abstractions.DomainModel;

namespace HexMaster.MemeIt.Games.DomainModels;

public class GameState : IGameState
{

    private List<IGamePlayer> _players = new();

    public IReadOnlyList<IGamePlayer> Players => _players.AsReadOnly();
    public IGameOptions Options { get; }

    public void AddPlayer(IGamePlayer player)
    {
        if (Players.Any(p => p.Id == player.Id))
        {
            throw new InvalidOperationException("Player already exists in the game.");
        }
        if (Players.Count >= Options.MaxPlayers)
        {
            throw new InvalidOperationException("Cannot add more players than the maximum allowed.");
        }
        _players.Add(player);
    }

    public bool RemovePlayer(IGamePlayer player)
    {
        return _players.Remove(player);
    }
}