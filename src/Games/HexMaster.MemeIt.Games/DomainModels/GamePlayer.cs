using HexMaster.MemeIt.Games.Abstractions.DomainModel;

namespace HexMaster.MemeIt.Games.DomainModels;

public class GamePlayer : IGamePlayer
{
    public Guid Id { get; }
    public string DisplayName { get; }
}