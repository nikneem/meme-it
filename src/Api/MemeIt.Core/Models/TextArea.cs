using Orleans;

namespace MemeIt.Core.Models;

/// <summary>
/// Represents a text area definition on a meme image
/// </summary>
[GenerateSerializer]
public record TextArea
{
    /// <summary>
    /// Gets the unique identifier for this text area
    /// </summary>
    [Id(1)]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the X coordinate of the text area
    /// </summary>
    [Id(2)]
    public required int X { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the text area
    /// </summary>
    [Id(3)]
    public required int Y { get; init; }

    /// <summary>
    /// Gets the width of the text area
    /// </summary>
    [Id(4)]
    public required int Width { get; init; }

    /// <summary>
    /// Gets the height of the text area
    /// </summary>
    [Id(5)]
    public required int Height { get; init; }

    /// <summary>
    /// Gets the font size for text in this area
    /// </summary>
    [Id(6)]
    public required int FontSize { get; init; }

    /// <summary>
    /// Gets the maximum number of characters allowed in this text area
    /// </summary>
    [Id(7)]
    public int MaxCharacters { get; init; } = 100;

    /// <summary>
    /// Gets the text alignment for this area
    /// </summary>
    [Id(8)]
    public TextAlignment Alignment { get; init; } = TextAlignment.Center;

    /// <summary>
    /// Gets the font color for this text area
    /// </summary>
    [Id(9)]
    public string FontColor { get; init; } = "#FFFFFF";

    /// <summary>
    /// Gets whether this text area has a stroke/outline
    /// </summary>
    [Id(10)]
    public bool HasStroke { get; init; } = true;

    /// <summary>
    /// Gets the stroke color for this text area
    /// </summary>
    [Id(11)]
    public string StrokeColor { get; init; } = "#000000";

    /// <summary>
    /// Gets the stroke width for this text area
    /// </summary>
    [Id(12)]
    public int StrokeWidth { get; init; } = 2;
}

/// <summary>
/// Defines text alignment options
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right
}
