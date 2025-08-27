namespace HexMaster.MemeIt.Games.DataTransferObjects;

public class GetRandomMemeResponse
{
    public string PlayerId { get; set; } = string.Empty;
    public string GameCode { get; set; } = string.Empty;
    public RandomMemeTemplateResponse MemeTemplate { get; set; } = new();
    public DateTime AssignedAt { get; set; }
}

public class RandomMemeTemplateResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int SourceWidth { get; set; }
    public int SourceHeight { get; set; }
    public List<RandomMemeTextAreaResponse> TextAreas { get; set; } = new();
}

public class RandomMemeTextAreaResponse
{
    public string Id { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int MaxLength { get; set; }
    public int FontSize { get; set; }
    public string FontFamily { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
    public string TextAlign { get; set; } = string.Empty;
    public string VerticalAlign { get; set; } = string.Empty;
}
