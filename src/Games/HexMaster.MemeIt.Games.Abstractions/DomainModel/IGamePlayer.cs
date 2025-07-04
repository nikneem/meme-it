namespace HexMaster.MemeIt.Games.Abstractions.DomainModel;

public interface IGamePlayer
{
    Guid Id { get; }
    string DisplayName { get; }
}