using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;

namespace HexMaster.MemeIt.Memes.Tests.Domains;

public class TextAreaDefinitionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTextAreaDefinition()
    {
        // Act
        var textArea = TextAreaDefinition.Create(10, 20, 200, 50, 24, "#FFFFFF", 2, "#000000", true);

        // Assert
        Assert.NotNull(textArea);
        Assert.Equal(10, textArea.X);
        Assert.Equal(20, textArea.Y);
        Assert.Equal(200, textArea.Width);
        Assert.Equal(50, textArea.Height);
        Assert.Equal(24, textArea.FontSize);
        Assert.Equal("#FFFFFF", textArea.FontColor);
        Assert.Equal(2, textArea.BorderSize);
        Assert.Equal("#000000", textArea.BorderColor);
        Assert.True(textArea.IsBold);
    }

    [Theory]
    [InlineData("#FFF")]
    [InlineData("#AABBCC")]
    [InlineData("#123")]
    public void Create_WithValidHexColors_ShouldNormalizeToUpperCase(string hexColor)
    {
        // Act
        var textArea = TextAreaDefinition.Create(0, 0, 100, 100, 20, hexColor.ToLower(), 1, hexColor.ToLower(), false);

        // Assert
        Assert.Equal(hexColor.ToUpper(), textArea.FontColor);
        Assert.Equal(hexColor.ToUpper(), textArea.BorderColor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidWidth_ShouldThrowDomainException(int width)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TextAreaDefinition.Create(0, 0, width, 100, 20, "#FFFFFF", 1, "#000000", false));
        Assert.Contains("Width must be greater than 0", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidHeight_ShouldThrowDomainException(int height)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TextAreaDefinition.Create(0, 0, 100, height, 20, "#FFFFFF", 1, "#000000", false));
        Assert.Contains("Height must be greater than 0", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidFontSize_ShouldThrowDomainException(int fontSize)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TextAreaDefinition.Create(0, 0, 100, 100, fontSize, "#FFFFFF", 1, "#000000", false));
        Assert.Contains("Font size must be greater than 0", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hex-color")]
    [InlineData("#GGGGGG")]
    [InlineData("FFFFFF")]
    public void Create_WithInvalidFontColor_ShouldThrowException(string color)
    {
        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            TextAreaDefinition.Create(0, 0, 100, 100, 20, color, 1, "#000000", false));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_WithNegativeBorderSize_ShouldThrowDomainException(int borderSize)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TextAreaDefinition.Create(0, 0, 100, 100, 20, "#FFFFFF", borderSize, "#000000", false));
        Assert.Contains("Border size cannot be negative", exception.Message);
    }
}
