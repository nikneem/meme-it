using HexMaster.MemeIt.Memes.Domains;

namespace HexMaster.MemeIt.Memes.Repositories;

/// <summary>
/// Repository interface for meme template aggregate root operations.
/// </summary>
public interface IMemeTemplateRepository
{
    /// <summary>
    /// Gets a meme template by its ID.
    /// </summary>
    Task<MemeTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all meme templates.
    /// </summary>
    Task<IReadOnlyList<MemeTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a random meme template.
    /// </summary>
    Task<MemeTemplate?> GetRandomAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new meme template.
    /// </summary>
    Task<Guid> AddAsync(MemeTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing meme template.
    /// </summary>
    Task UpdateAsync(MemeTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a meme template by its ID.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a template exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
