using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace HexMaster.MemeIt.Memes.Models;

public class MemeTemplate
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("sourceImageUrl")]
    public string SourceImageUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("sourceWidth")]
    public int SourceWidth { get; set; }
    
    [JsonPropertyName("sourceHeight")]
    public int SourceHeight { get; set; }
    
    [JsonPropertyName("textAreas")]
    public TextArea[] TextAreas { get; set; } = Array.Empty<TextArea>();
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }
    
    public string PartitionKey => MemesConstants.CosmosDbPartitionKey;
}

public class TextArea
{
    [JsonPropertyName("x")]
    public int X { get; set; }
    
    [JsonPropertyName("y")]
    public int Y { get; set; }
    
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
    
    [JsonPropertyName("fontFamily")]
    public string FontFamily { get; set; } = string.Empty;
    
    [JsonPropertyName("fontSize")]
    public int FontSize { get; set; }
    
    [JsonPropertyName("fontColor")]
    public string FontColor { get; set; } = string.Empty;
    
    [JsonPropertyName("maxLength")]
    public int MaxLength { get; set; }
}
