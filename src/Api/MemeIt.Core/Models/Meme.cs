namespace MemeIt.Core.Models;

/// <summary>
/// Represents a meme template with image and text areas
/// </summary>
public record Meme
{
    /// <summary>
    /// Gets the unique identifier for this meme
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name/title of this meme
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the URL where the meme image is stored
    /// </summary>
    public required string ImageUrl { get; init; }

    /// <summary>
    /// Gets the collection of text areas for this meme
    /// </summary>
    public required IReadOnlyList<TextArea> TextAreas { get; init; }

    /// <summary>
    /// Gets the categories this meme belongs to
    /// </summary>
    public required IReadOnlyList<string> Categories { get; init; }

    /// <summary>
    /// Gets the tags associated with this meme
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the width of the meme image in pixels
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Gets the height of the meme image in pixels
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the last modified timestamp
    /// </summary>
    public DateTimeOffset ModifiedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets whether this meme is active/available for use
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the difficulty level of this meme (1-5)
    /// </summary>
    public int DifficultyLevel { get; init; } = 1;

    /// <summary>
    /// Gets the popularity score based on usage
    /// </summary>
    public int PopularityScore { get; init; } = 0;
}
