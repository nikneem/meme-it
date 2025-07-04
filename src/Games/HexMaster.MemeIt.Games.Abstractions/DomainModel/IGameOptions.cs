namespace HexMaster.MemeIt.Games.Abstractions.DomainModel;

public interface IGameOptions
{
    int Rounds { get; }
    int MaxPlayers { get; }
    int MinPlayers { get; }

}