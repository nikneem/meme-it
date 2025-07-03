using System.Text.Json.Serialization;
using MemeIt.Core.Models;

namespace MemeIt.Library.Infrastructure.Models;

/// <summary>
/// CosmosDB document model for storing memes
/// </summary>
public class MemeDocument
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("partitionKey")]
    public required string PartitionKey { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("imageUrl")]
    public required string ImageUrl { get; set; }

    [JsonPropertyName("textAreas")]
    public required List<TextAreaDocument> TextAreas { get; set; }

    [JsonPropertyName("categories")]
    public required List<string> Categories { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("width")]
    public required int Width { get; set; }

    [JsonPropertyName("height")]
    public required int Height { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("modifiedAt")]
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("difficultyLevel")]
    public int DifficultyLevel { get; set; } = 1;

    [JsonPropertyName("popularityScore")]
    public int PopularityScore { get; set; } = 0;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "meme";

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    /// <summary>
    /// Converts the document to a domain model
    /// </summary>
    public Meme ToDomain()
    {
        return new Meme
        {
            Id = Id,
            Name = Name,
            ImageUrl = ImageUrl,
            TextAreas = TextAreas.Select(ta => ta.ToDomain()).ToList(),
            Categories = Categories,
            Tags = Tags,
            Width = Width,
            Height = Height,
            CreatedAt = CreatedAt,
            ModifiedAt = ModifiedAt,
            IsActive = IsActive,
            DifficultyLevel = DifficultyLevel,
            PopularityScore = PopularityScore
        };
    }

    /// <summary>
    /// Creates a document from a domain model
    /// </summary>
    public static MemeDocument FromDomain(Meme meme)
    {
        return new MemeDocument
        {
            Id = meme.Id,
            PartitionKey = GeneratePartitionKey(meme.Categories),
            Name = meme.Name,
            ImageUrl = meme.ImageUrl,
            TextAreas = meme.TextAreas.Select(TextAreaDocument.FromDomain).ToList(),
            Categories = meme.Categories.ToList(),
            Tags = meme.Tags.ToList(),
            Width = meme.Width,
            Height = meme.Height,
            CreatedAt = meme.CreatedAt,
            ModifiedAt = meme.ModifiedAt,
            IsActive = meme.IsActive,
            DifficultyLevel = meme.DifficultyLevel,
            PopularityScore = meme.PopularityScore
        };
    }

    /// <summary>
    /// Generates a partition key based on primary category
    /// </summary>
    private static string GeneratePartitionKey(IReadOnlyList<string> categories)
    {
        return categories.Count > 0 ? categories[0] : "default";
    }
}

/// <summary>
/// CosmosDB document model for text areas
/// </summary>
public class TextAreaDocument
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }

    [JsonPropertyName("width")]
    public required int Width { get; set; }

    [JsonPropertyName("height")]
    public required int Height { get; set; }

    [JsonPropertyName("fontSize")]
    public required int FontSize { get; set; }

    [JsonPropertyName("maxCharacters")]
    public int MaxCharacters { get; set; } = 100;

    [JsonPropertyName("alignment")]
    public string Alignment { get; set; } = "Center";

    [JsonPropertyName("fontColor")]
    public string FontColor { get; set; } = "#FFFFFF";

    [JsonPropertyName("hasStroke")]
    public bool HasStroke { get; set; } = true;

    [JsonPropertyName("strokeColor")]
    public string StrokeColor { get; set; } = "#000000";

    [JsonPropertyName("strokeWidth")]
    public int StrokeWidth { get; set; } = 2;

    /// <summary>
    /// Converts the document to a domain model
    /// </summary>
    public TextArea ToDomain()
    {
        return new TextArea
        {
            Id = Id,
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            FontSize = FontSize,
            MaxCharacters = MaxCharacters,
            Alignment = Enum.Parse<TextAlignment>(Alignment),
            FontColor = FontColor,
            HasStroke = HasStroke,
            StrokeColor = StrokeColor,
            StrokeWidth = StrokeWidth
        };
    }

    /// <summary>
    /// Creates a document from a domain model
    /// </summary>
    public static TextAreaDocument FromDomain(TextArea textArea)
    {
        return new TextAreaDocument
        {
            Id = textArea.Id,
            X = textArea.X,
            Y = textArea.Y,
            Width = textArea.Width,
            Height = textArea.Height,
            FontSize = textArea.FontSize,
            MaxCharacters = textArea.MaxCharacters,
            Alignment = textArea.Alignment.ToString(),
            FontColor = textArea.FontColor,
            HasStroke = textArea.HasStroke,
            StrokeColor = textArea.StrokeColor,
            StrokeWidth = textArea.StrokeWidth
        };
    }
}
