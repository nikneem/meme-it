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

    /// <summary>
    /// Retrieves a game by its unique game code.
    /// </summary>
    /// <param name="gameCode">The game code to search for.</param>
    /// <param name="cancellationToken">Cancellation notification token.</param>
    /// <returns>Game aggregate if found; otherwise null.</returns>
    Task<IGame?> GetByGameCodeAsync(string gameCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing game aggregate.
    /// </summary>
    /// <param name="game">Aggregate state to persist.</param>
    /// <param name="cancellationToken">Cancellation notification token.</param>
    Task UpdateAsync(IGame game, CancellationToken cancellationToken = default);
}
