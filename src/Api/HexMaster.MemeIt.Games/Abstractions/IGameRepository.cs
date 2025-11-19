using HexMaster.MemeIt.Games.Domain;

namespace HexMaster.MemeIt.Games.Abstractions;

public interface IGameRepository
{
    Task<Game?> GetAsync(string gameCode, CancellationToken cancellationToken = default);
    Task SaveAsync(Game game, CancellationToken cancellationToken = default);
    Task DeleteAsync(string gameCode, CancellationToken cancellationToken = default);
}
