using MemeIt.Core.Models;

namespace MemeIt.Library.Abstractions;

/// <summary>
/// Defines the contract for the meme library service
/// </summary>
public interface IMemeLibraryService
{
    /// <summary>
    /// Gets a random meme for a player from the specified categories
    /// </summary>
    /// <param name="playerId">The player identifier</param>
    /// <param name="categories">The categories to select from (if empty, all categories are used)</param>
    /// <param name="excludedMemeIds">Meme IDs to exclude from selection</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A random meme or null if none available</returns>
    Task<Meme?> GetRandomMemeForPlayerAsync(
        string playerId,
        IReadOnlyList<string> categories,
        IReadOnlyList<string> excludedMemeIds = null!,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a meme by its identifier
    /// </summary>
    /// <param name="memeId">The meme identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The meme or null if not found</returns>
    Task<Meme?> GetMemeByIdAsync(string memeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available categories</returns>
    Task<IReadOnlyList<MemeCategory>> GetAvailableCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records that a meme was used in a game (for popularity tracking)
    /// </summary>
    /// <param name="memeId">The meme identifier</param>
    /// <param name="playerId">The player identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task RecordMemeUsageAsync(string memeId, string playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the specified categories exist and are active
    /// </summary>
    /// <param name="categories">The categories to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of valid categories</returns>
    Task<IReadOnlyList<string>> ValidateCategoriesAsync(
        IReadOnlyList<string> categories, 
        CancellationToken cancellationToken = default);
}
