using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Abstractions.Repositories;

/// <summary>
/// Persistence boundary for storing and retrieving games.
/// </summary>
public interface IGamesRepository
{
    /// <summary>
    /// Persists a new game aggregate.
    /// </summary>
    /// <param name="game">Aggregate state.</param>
    /// <param name="cancellationToken">Cancellation notification token.</param>
    Task CreateAsync(IGame game, CancellationToken cancellationToken = default);
}
