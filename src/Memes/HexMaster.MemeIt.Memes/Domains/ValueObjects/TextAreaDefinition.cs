namespace HexMaster.MemeIt.Memes.Domains.ValueObjects;

/// <summary>
/// Value object representing a text area definition on a meme template.
/// Contains position, dimensions, and styling information.
/// </summary>
public record TextAreaDefinition
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int FontSize { get; init; }
    public string FontColor { get; init; } = default!;
    public int BorderSize { get; init; }
    public string BorderColor { get; init; } = default!;
    public bool IsBold { get; init; }

    private TextAreaDefinition() { }

    /// <summary>
    /// Factory method to create a text area definition with validation.
    /// </summary>
    public static TextAreaDefinition Create(
        int x,
        int y,
        int width,
        int height,
        int fontSize,
        string fontColor,
        int borderSize,
        string borderColor,
        bool isBold)
    {
        if (width <= 0)
            throw new DomainException("Width must be greater than 0");

        if (height <= 0)
            throw new DomainException("Height must be greater than 0");

        if (fontSize <= 0)
            throw new DomainException("Font size must be greater than 0");

        ArgumentException.ThrowIfNullOrWhiteSpace(fontColor, nameof(fontColor));
        ArgumentException.ThrowIfNullOrWhiteSpace(borderColor, nameof(borderColor));

        if (!IsValidHexColor(fontColor))
            throw new DomainException("Font color must be a valid hex color (e.g., #FFFFFF)");

        if (!IsValidHexColor(borderColor))
            throw new DomainException("Border color must be a valid hex color (e.g., #000000)");

        if (borderSize < 0)
            throw new DomainException("Border size cannot be negative");

        return new TextAreaDefinition
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            FontSize = fontSize,
            FontColor = fontColor.ToUpperInvariant(),
            BorderSize = borderSize,
            BorderColor = borderColor.ToUpperInvariant(),
            IsBold = isBold
        };
    }

    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        // Support both #RGB and #RRGGBB formats
        return System.Text.RegularExpressions.Regex.IsMatch(
            color,
            @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$");
    }
}
