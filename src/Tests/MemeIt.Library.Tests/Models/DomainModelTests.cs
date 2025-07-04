using MemeIt.Core.Models;
using MemeIt.Library.Tests.TestData;

namespace MemeIt.Library.Tests.Models;

public class MemeTests
{
    [Fact]
    public void Meme_Creation_WithRequiredProperties_ShouldSucceed()
    {
        // Arrange & Act
        var meme = MemeTestDataFactory.CreateSampleMeme();

        // Assert
        Assert.NotNull(meme);
        Assert.False(string.IsNullOrEmpty(meme.Id));
        Assert.False(string.IsNullOrEmpty(meme.Name));
        Assert.False(string.IsNullOrEmpty(meme.ImageUrl));
        Assert.NotEmpty(meme.TextAreas);
        Assert.NotEmpty(meme.Categories);
        Assert.True(meme.Width > 0);
        Assert.True(meme.Height > 0);
    }

    [Fact]
    public void Meme_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var meme = MemeTestDataFactory.CreateSampleMeme();

        // Assert
        Assert.True(meme.IsActive);
        Assert.Equal(1, meme.DifficultyLevel);
        Assert.True(meme.PopularityScore >= 0);
        Assert.NotNull(meme.Tags);
    }

    [Fact]
    public void Meme_WithCategories_ShouldContainExpectedCategories()
    {
        // Arrange
        var expectedCategories = new List<string> { "humor", "classic", "animals" };

        // Act
        var meme = MemeTestDataFactory.CreateSampleMeme(categories: expectedCategories);

        // Assert
        Assert.Equal(expectedCategories.Count, meme.Categories.Count);
        foreach (var category in expectedCategories)
        {
            Assert.Contains(category, meme.Categories);
        }
    }

    [Fact]
    public void Meme_WithTextAreas_ShouldContainExpectedTextAreas()
    {
        // Arrange
        var expectedTextAreas = MemeTestDataFactory.CreateSampleTextAreas();

        // Act
        var meme = MemeTestDataFactory.CreateSampleMeme(textAreas: expectedTextAreas);

        // Assert
        Assert.Equal(expectedTextAreas.Count, meme.TextAreas.Count);
        for (int i = 0; i < expectedTextAreas.Count; i++)
        {
            Assert.Equal(expectedTextAreas[i].Id, meme.TextAreas[i].Id);
            Assert.Equal(expectedTextAreas[i].X, meme.TextAreas[i].X);
            Assert.Equal(expectedTextAreas[i].Y, meme.TextAreas[i].Y);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Meme_IsActive_ShouldReflectProvidedValue(bool isActive)
    {
        // Arrange & Act
        var meme = MemeTestDataFactory.CreateSampleMeme(isActive: isActive);

        // Assert
        Assert.Equal(isActive, meme.IsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    public void Meme_PopularityScore_ShouldReflectProvidedValue(int popularityScore)
    {
        // Arrange & Act
        var meme = MemeTestDataFactory.CreateSampleMeme(popularityScore: popularityScore);

        // Assert
        Assert.Equal(popularityScore, meme.PopularityScore);
    }
}

public class TextAreaTests
{
    [Fact]
    public void TextArea_Creation_WithRequiredProperties_ShouldSucceed()
    {
        // Arrange & Act
        var textArea = MemeTestDataFactory.CreateSampleTextArea();

        // Assert
        Assert.NotNull(textArea);
        Assert.False(string.IsNullOrEmpty(textArea.Id));
        Assert.True(textArea.X >= 0);
        Assert.True(textArea.Y >= 0);
        Assert.True(textArea.Width > 0);
        Assert.True(textArea.Height > 0);
        Assert.True(textArea.FontSize > 0);
    }

    [Fact]
    public void TextArea_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var textArea = MemeTestDataFactory.CreateSampleTextArea();

        // Assert
        Assert.Equal(100, textArea.MaxCharacters);
        Assert.Equal(TextAlignment.Center, textArea.Alignment);
        Assert.Equal("#FFFFFF", textArea.FontColor);
        Assert.True(textArea.HasStroke);
        Assert.Equal("#000000", textArea.StrokeColor);
        Assert.Equal(2, textArea.StrokeWidth);
    }

    [Theory]
    [InlineData(TextAlignment.Left)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.Right)]
    public void TextArea_Alignment_ShouldSupportAllValues(TextAlignment alignment)
    {
        // Arrange & Act
        var textArea = new TextArea
        {
            Id = "test",
            X = 0,
            Y = 0,
            Width = 100,
            Height = 50,
            FontSize = 16,
            Alignment = alignment
        };

        // Assert
        Assert.Equal(alignment, textArea.Alignment);
    }

    [Theory]
    [InlineData(10, 20, 200, 100, 24)]
    [InlineData(0, 0, 50, 25, 12)]
    [InlineData(100, 150, 300, 80, 36)]
    public void TextArea_Position_ShouldReflectProvidedValues(int x, int y, int width, int height, int fontSize)
    {
        // Arrange & Act
        var textArea = MemeTestDataFactory.CreateSampleTextArea(x: x, y: y, width: width, height: height, fontSize: fontSize);

        // Assert
        Assert.Equal(x, textArea.X);
        Assert.Equal(y, textArea.Y);
        Assert.Equal(width, textArea.Width);
        Assert.Equal(height, textArea.Height);
        Assert.Equal(fontSize, textArea.FontSize);
    }
}

public class MemeCategoryTests
{
    [Fact]
    public void MemeCategory_Creation_WithRequiredProperties_ShouldSucceed()
    {
        // Arrange & Act
        var category = MemeTestDataFactory.CreateSampleCategory();

        // Assert
        Assert.NotNull(category);
        Assert.False(string.IsNullOrEmpty(category.Id));
        Assert.False(string.IsNullOrEmpty(category.Name));
    }

    [Fact]
    public void MemeCategory_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var category = MemeTestDataFactory.CreateSampleCategory();

        // Assert
        Assert.True(category.IsActive);
        Assert.True(category.DisplayOrder >= 0);
        Assert.NotNull(category.Description);
        Assert.False(string.IsNullOrEmpty(category.Color));
        Assert.NotNull(category.Icon);
        var timeDifference = Math.Abs((DateTimeOffset.UtcNow - category.CreatedAt).TotalDays);
        Assert.True(timeDifference <= 90);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MemeCategory_IsActive_ShouldReflectProvidedValue(bool isActive)
    {
        // Arrange & Act
        var category = MemeTestDataFactory.CreateSampleCategory(isActive: isActive);

        // Assert
        Assert.Equal(isActive, category.IsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void MemeCategory_DisplayOrder_ShouldReflectProvidedValue(int displayOrder)
    {
        // Arrange & Act
        var category = MemeTestDataFactory.CreateSampleCategory(displayOrder: displayOrder);

        // Assert
        Assert.Equal(displayOrder, category.DisplayOrder);
    }

    [Fact]
    public void MemeCategory_MultipleCategories_ShouldBeUnique()
    {
        // Arrange & Act
        var categories = MemeTestDataFactory.CreateMultipleCategories();

        // Assert
        Assert.NotEmpty(categories);
        
        // Check unique IDs
        var ids = categories.Select(c => c.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
        
        // Check unique names
        var names = categories.Select(c => c.Name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
