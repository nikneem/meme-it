using FluentAssertions;
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
        textArea.Should().NotBeNull();
        textArea.X.Should().Be(10);
        textArea.Y.Should().Be(20);
        textArea.Width.Should().Be(200);
        textArea.Height.Should().Be(50);
        textArea.FontSize.Should().Be(24);
        textArea.FontColor.Should().Be("#FFFFFF");
        textArea.BorderSize.Should().Be(2);
        textArea.BorderColor.Should().Be("#000000");
        textArea.IsBold.Should().BeTrue();
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
        textArea.FontColor.Should().Be(hexColor.ToUpper());
        textArea.BorderColor.Should().Be(hexColor.ToUpper());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidWidth_ShouldThrowDomainException(int width)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            TextAreaDefinition.Create(0, 0, width, 100, 20, "#FFFFFF", 1, "#000000", false));
        exception.Message.Should().Contain("Width must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidHeight_ShouldThrowDomainException(int height)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            TextAreaDefinition.Create(0, 0, 100, height, 20, "#FFFFFF", 1, "#000000", false));
        exception.Message.Should().Contain("Height must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidFontSize_ShouldThrowDomainException(int fontSize)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            TextAreaDefinition.Create(0, 0, 100, 100, fontSize, "#FFFFFF", 1, "#000000", false));
        exception.Message.Should().Contain("Font size must be greater than 0");
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
        exception.Message.Should().Contain("Border size cannot be negative");
    }
}
