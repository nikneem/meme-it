using Orleans;

namespace MemeIt.Core.Models;

/// <summary>
/// Represents a meme category
/// </summary>
[GenerateSerializer]
public record MemeCategory
{
    /// <summary>
    /// Gets the unique identifier for this category
    /// </summary>
    [Id(1)]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name of this category
    /// </summary>
    [Id(2)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of this category
    /// </summary>
    [Id(3)]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this category is active
    /// </summary>
    [Id(4)]
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the display order for this category
    /// </summary>
    [Id(5)]
    public int DisplayOrder { get; init; } = 0;

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    [Id(6)]
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the color associated with this category (hex format)
    /// </summary>
    [Id(7)]
    public string Color { get; init; } = "#000000";

    /// <summary>
    /// Gets the icon name for this category
    /// </summary>
    [Id(8)]
    public string Icon { get; init; } = string.Empty;
}
