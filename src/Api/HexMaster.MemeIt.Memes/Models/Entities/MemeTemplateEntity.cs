using System.Text.Json.Serialization;

namespace HexMaster.MemeIt.Memes.Models.Entities;

/// <summary>
/// CosmosDB entity for meme template optimized for storage
/// </summary>
public class MemeTemplateEntity
{
    [JsonPropertyName("id")] // CosmosDB requires lowercase 'id'
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Contains only the filename (e.g., "obama.jpg"), not the full URL.
    /// Full URL is constructed by combining blob storage base URL + container name + filename.
    /// </summary>
    public string SourceImageUrl { get; set; } = string.Empty;
    
    public int SourceWidth { get; set; }
    
    public int SourceHeight { get; set; }
    
    public TextAreaEntity[] TextAreas { get; set; } = Array.Empty<TextAreaEntity>();
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    [JsonPropertyName("_etag")] // CosmosDB system property requires underscore prefix
    public string? ETag { get; set; }
    
    public string PartitionKey { get; set; } = MemesConstants.CosmosDbPartitionKey;
}

/// <summary>
/// CosmosDB entity for text area optimized for storage
/// </summary>
public class TextAreaEntity
{
    public int X { get; set; }
    
    public int Y { get; set; }
    
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public string FontFamily { get; set; } = string.Empty;
    
    public int FontSize { get; set; }
    
    public string FontColor { get; set; } = string.Empty;
    
    public bool FontBold { get; set; }
    
    public int MaxLength { get; set; }
    
    public int BorderThickness { get; set; }
    
    public string BorderColor { get; set; } = string.Empty;
}
