using MemeIt.Core.Models;

namespace MemeIt.Library.Abstractions;

/// <summary>
/// Defines the contract for meme repository operations
/// </summary>
public interface IMemeRepository
{
    /// <summary>
    /// Gets a random meme from the specified categories
    /// </summary>
    /// <param name="categories">The categories to select from</param>
    /// <param name="excludedMemeIds">Meme IDs to exclude from selection</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A random meme or null if none found</returns>
    Task<Meme?> GetRandomMemeAsync(
        IReadOnlyList<string> categories, 
        IReadOnlyList<string> excludedMemeIds = null!,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a meme by its unique identifier
    /// </summary>
    /// <param name="id">The meme identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The meme or null if not found</returns>
    Task<Meme?> GetMemeByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all memes in the specified categories
    /// </summary>
    /// <param name="categories">The categories to filter by</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of memes</returns>
    Task<IReadOnlyList<Meme>> GetMemesByCategoriesAsync(
        IReadOnlyList<string> categories,
        bool? isActive = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new meme
    /// </summary>
    /// <param name="meme">The meme to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created meme</returns>
    Task<Meme> CreateMemeAsync(Meme meme, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing meme
    /// </summary>
    /// <param name="meme">The meme to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated meme</returns>
    Task<Meme> UpdateMemeAsync(Meme meme, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a meme by its identifier
    /// </summary>
    /// <param name="id">The meme identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteMemeAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the popularity score of a meme
    /// </summary>
    /// <param name="memeId">The meme identifier</param>
    /// <param name="increment">The amount to increment the score by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated popularity score</returns>
    Task<int> UpdatePopularityScoreAsync(string memeId, int increment = 1, CancellationToken cancellationToken = default);
}
