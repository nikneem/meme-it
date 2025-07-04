using Orleans;

namespace MemeIt.Core.Models;

/// <summary>
/// Represents a meme template with image and text areas
/// </summary>
[GenerateSerializer]
public record Meme
{
    /// <summary>
    /// Gets the unique identifier for this meme
    /// </summary>
    [Id(1)]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name/title of this meme
    /// </summary>
    [Id(2)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the URL where the meme image is stored
    /// </summary>
    [Id(3)]
    public required string ImageUrl { get; init; }

    /// <summary>
    /// Gets the collection of text areas for this meme
    /// </summary>
    [Id(4)]
    public required IReadOnlyList<TextArea> TextAreas { get; init; }

    /// <summary>
    /// Gets the categories this meme belongs to
    /// </summary>
    [Id(5)]
    public required IReadOnlyList<string> Categories { get; init; }

    /// <summary>
    /// Gets the tags associated with this meme
    /// </summary>
    [Id(6)]
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the width of the meme image in pixels
    /// </summary>
    [Id(7)]
    public required int Width { get; init; }

    /// <summary>
    /// Gets the height of the meme image in pixels
    /// </summary>
    [Id(8)]
    public required int Height { get; init; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    [Id(9)]
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the last modified timestamp
    /// </summary>
    [Id(10)]
    public DateTimeOffset ModifiedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets whether this meme is active/available for use
    /// </summary>
    [Id(11)]
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the difficulty level of this meme (1-5)
    /// </summary>
    [Id(12)]
    public int DifficultyLevel { get; init; } = 1;

    /// <summary>
    /// Gets the popularity score based on usage
    /// </summary>
    [Id(13)]
    public int PopularityScore { get; init; } = 0;
}
