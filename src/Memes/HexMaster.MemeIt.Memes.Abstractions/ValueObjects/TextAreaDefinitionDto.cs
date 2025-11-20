namespace HexMaster.MemeIt.Memes.Abstractions.ValueObjects;

/// <summary>
/// DTO representing a text area definition.
/// </summary>
public record TextAreaDefinitionDto(
    int X,
    int Y,
    int Width,
    int Height,
    int FontSize,
    string FontColor,
    int BorderSize,
    string BorderColor,
    bool IsBold
);
