namespace HexMaster.MemeIt.Memes.Models;

/// <summary>
/// Domain model for a meme template with proper encapsulation and validation
/// </summary>
public class MemeTemplate
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string SourceImageUrl { get; private set; }
    public int SourceWidth { get; private set; }
    public int SourceHeight { get; private set; }
    public IReadOnlyList<TextArea> TextAreas { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? ETag { get; private set; }

    // Constructor for creating new meme templates
    public MemeTemplate(string name, string? description, string sourceImageUrl, int sourceWidth, int sourceHeight, IEnumerable<TextArea> textAreas)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(sourceImageUrl))
            throw new ArgumentException("Source image URL cannot be null or empty", nameof(sourceImageUrl));
        if (sourceWidth <= 0)
            throw new ArgumentException("Source width must be greater than 0", nameof(sourceWidth));
        if (sourceHeight <= 0)
            throw new ArgumentException("Source height must be greater than 0", nameof(sourceHeight));

        Id = Guid.NewGuid().ToString();
        Name = name;
        Description = description;
        SourceImageUrl = sourceImageUrl;
        SourceWidth = sourceWidth;
        SourceHeight = sourceHeight;
        TextAreas = textAreas?.ToList().AsReadOnly() ?? new List<TextArea>().AsReadOnly();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = null;
        ETag = null;
    }

    // Constructor for loading existing meme templates from storage
    internal MemeTemplate(string id, string name, string? description, string sourceImageUrl, int sourceWidth, int sourceHeight, 
        IEnumerable<TextArea> textAreas, DateTimeOffset createdAt, DateTimeOffset? updatedAt, string? etag)
    {
        Id = id;
        Name = name;
        Description = description;
        SourceImageUrl = sourceImageUrl;
        SourceWidth = sourceWidth;
        SourceHeight = sourceHeight;
        TextAreas = textAreas?.ToList().AsReadOnly() ?? new List<TextArea>().AsReadOnly();
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ETag = etag;
    }

    public void Update(string name, string? description, string sourceImageUrl, int sourceWidth, int sourceHeight, IEnumerable<TextArea> textAreas)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(sourceImageUrl))
            throw new ArgumentException("Source image URL cannot be null or empty", nameof(sourceImageUrl));
        if (sourceWidth <= 0)
            throw new ArgumentException("Source width must be greater than 0", nameof(sourceWidth));
        if (sourceHeight <= 0)
            throw new ArgumentException("Source height must be greater than 0", nameof(sourceHeight));

        Name = name;
        Description = description;
        SourceImageUrl = sourceImageUrl;
        SourceWidth = sourceWidth;
        SourceHeight = sourceHeight;
        TextAreas = textAreas?.ToList().AsReadOnly() ?? new List<TextArea>().AsReadOnly();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void SetETag(string? etag)
    {
        ETag = etag;
    }
}

/// <summary>
/// Domain model for text area with proper encapsulation and validation
/// </summary>
public class TextArea
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string FontFamily { get; private set; }
    public int FontSize { get; private set; }
    public string FontColor { get; private set; }
    public bool FontBold { get; private set; }
    public int MaxLength { get; private set; }
    public int BorderThickness { get; private set; }
    public string BorderColor { get; private set; }

    public TextArea(int x, int y, int width, int height, string fontFamily, int fontSize, string fontColor, 
        bool fontBold, int maxLength, int borderThickness, string borderColor)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be greater than 0", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be greater than 0", nameof(height));
        if (string.IsNullOrWhiteSpace(fontFamily))
            throw new ArgumentException("Font family cannot be null or empty", nameof(fontFamily));
        if (fontSize <= 0)
            throw new ArgumentException("Font size must be greater than 0", nameof(fontSize));
        if (string.IsNullOrWhiteSpace(fontColor))
            throw new ArgumentException("Font color cannot be null or empty", nameof(fontColor));
        if (maxLength <= 0)
            throw new ArgumentException("Max length must be greater than 0", nameof(maxLength));
        if (borderThickness < 0)
            throw new ArgumentException("Border thickness cannot be negative", nameof(borderThickness));
        if (string.IsNullOrWhiteSpace(borderColor))
            throw new ArgumentException("Border color cannot be null or empty", nameof(borderColor));

        X = x;
        Y = y;
        Width = width;
        Height = height;
        FontFamily = fontFamily;
        FontSize = fontSize;
        FontColor = fontColor;
        FontBold = fontBold;
        MaxLength = maxLength;
        BorderThickness = borderThickness;
        BorderColor = borderColor;
    }
}
