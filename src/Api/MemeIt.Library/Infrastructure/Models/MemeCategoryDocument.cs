using System.Text.Json.Serialization;
using MemeIt.Core.Models;

namespace MemeIt.Library.Infrastructure.Models;

/// <summary>
/// CosmosDB document model for storing meme categories
/// </summary>
public class MemeCategoryDocument
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("partitionKey")]
    public required string PartitionKey { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("displayOrder")]
    public int DisplayOrder { get; set; } = 0;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#000000";

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = nameof(MemeCategoryDocument) ;

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    /// <summary>
    /// Converts the document to a domain model
    /// </summary>
    public MemeCategory ToDomain()
    {
        return new MemeCategory
        {
            Id = Id,
            Name = Name,
            Description = Description,
            IsActive = IsActive,
            DisplayOrder = DisplayOrder,
            CreatedAt = CreatedAt,
            Color = Color,
            Icon = Icon
        };
    }

    /// <summary>
    /// Creates a document from a domain model
    /// </summary>
    public static MemeCategoryDocument FromDomain(MemeCategory category)
    {
        return new MemeCategoryDocument
        {
            Id = category.Id,
            PartitionKey = "category",
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            DisplayOrder = category.DisplayOrder,
            CreatedAt = category.CreatedAt,
            Color = category.Color,
            Icon = category.Icon
        };
    }
}
