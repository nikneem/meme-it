namespace HexMaster.MemeIt.Memes.Models.Factories;

/// <summary>
/// Factory for creating meme templates with validation
/// </summary>
public static class MemeTemplateFactory
{
    /// <summary>
    /// Creates a new meme template with basic validation
    /// </summary>
    public static MemeTemplate Create(string name, string? description, string sourceImageUrl, 
        int sourceWidth, int sourceHeight, params TextArea[] textAreas)
    {
        return new MemeTemplate(name, description, sourceImageUrl, sourceWidth, sourceHeight, textAreas);
    }

    /// <summary>
    /// Creates a new meme template from existing template for editing
    /// </summary>
    public static MemeTemplate CreateFromExisting(MemeTemplate existing, string name, string? description, 
        string sourceImageUrl, int sourceWidth, int sourceHeight, params TextArea[] textAreas)
    {
        if (existing == null)
            throw new ArgumentNullException(nameof(existing));

        var newTemplate = new MemeTemplate(name, description, sourceImageUrl, sourceWidth, sourceHeight, textAreas);
        
        // Preserve the original ID and timestamps when creating an updated version
        return new MemeTemplate(
            existing.Id,
            name,
            description,
            sourceImageUrl,
            sourceWidth,
            sourceHeight,
            textAreas,
            existing.CreatedAt,
            DateTimeOffset.UtcNow, // Set updated time
            existing.ETag);
    }
}

/// <summary>
/// Factory for creating text areas with validation
/// </summary>
public static class TextAreaFactory
{
    /// <summary>
    /// Creates a text area with default styling
    /// </summary>
    public static TextArea CreateDefault(int x, int y, int width, int height, int maxLength = 100)
    {
        return new TextArea(x, y, width, height, "Arial", 24, "#FFFFFF", true, maxLength, 2, "#000000");
    }

    /// <summary>
    /// Creates a text area with custom styling
    /// </summary>
    public static TextArea CreateWithStyling(int x, int y, int width, int height, string fontFamily, 
        int fontSize, string fontColor, bool fontBold, int maxLength, int borderThickness, string borderColor)
    {
        return new TextArea(x, y, width, height, fontFamily, fontSize, fontColor, fontBold, maxLength, borderThickness, borderColor);
    }
}
