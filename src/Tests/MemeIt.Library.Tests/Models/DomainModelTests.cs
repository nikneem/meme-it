using FluentAssertions;
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
        meme.Should().NotBeNull();
        meme.Id.Should().NotBeNullOrEmpty();
        meme.Name.Should().NotBeNullOrEmpty();
        meme.ImageUrl.Should().NotBeNullOrEmpty();
        meme.TextAreas.Should().NotBeEmpty();
        meme.Categories.Should().NotBeEmpty();
        meme.Width.Should().BeGreaterThan(0);
        meme.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Meme_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var meme = MemeTestDataFactory.CreateSampleMeme();

        // Assert
        meme.IsActive.Should().BeTrue();
        meme.DifficultyLevel.Should().Be(1);
        meme.PopularityScore.Should().BeGreaterThanOrEqualTo(0);
        meme.Tags.Should().NotBeNull();
    }

    [Fact]
    public void Meme_WithCategories_ShouldContainExpectedCategories()
    {
        // Arrange
        var expectedCategories = new List<string> { "humor", "classic", "animals" };

        // Act
        var meme = MemeTestDataFactory.CreateSampleMeme(categories: expectedCategories);

        // Assert
        meme.Categories.Should().BeEquivalentTo(expectedCategories);
    }

    [Fact]
    public void Meme_WithTextAreas_ShouldContainExpectedTextAreas()
    {
        // Arrange
        var expectedTextAreas = MemeTestDataFactory.CreateSampleTextAreas();

        // Act
        var meme = MemeTestDataFactory.CreateSampleMeme(textAreas: expectedTextAreas);

        // Assert
        meme.TextAreas.Should().HaveCount(expectedTextAreas.Count);
        meme.TextAreas.Should().BeEquivalentTo(expectedTextAreas);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Meme_IsActive_ShouldReflectProvidedValue(bool isActive)
    {
        // Arrange & Act
        var meme = MemeTestDataFactory.CreateSampleMeme(isActive: isActive);

        // Assert
        meme.IsActive.Should().Be(isActive);
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
        meme.PopularityScore.Should().Be(popularityScore);
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
        textArea.Should().NotBeNull();
        textArea.Id.Should().NotBeNullOrEmpty();
        textArea.X.Should().BeGreaterThanOrEqualTo(0);
        textArea.Y.Should().BeGreaterThanOrEqualTo(0);
        textArea.Width.Should().BeGreaterThan(0);
        textArea.Height.Should().BeGreaterThan(0);
        textArea.FontSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void TextArea_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var textArea = MemeTestDataFactory.CreateSampleTextArea();

        // Assert
        textArea.MaxCharacters.Should().Be(100);
        textArea.Alignment.Should().Be(TextAlignment.Center);
        textArea.FontColor.Should().Be("#FFFFFF");
        textArea.HasStroke.Should().BeTrue();
        textArea.StrokeColor.Should().Be("#000000");
        textArea.StrokeWidth.Should().Be(2);
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
        textArea.Alignment.Should().Be(alignment);
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
        textArea.X.Should().Be(x);
        textArea.Y.Should().Be(y);
        textArea.Width.Should().Be(width);
        textArea.Height.Should().Be(height);
        textArea.FontSize.Should().Be(fontSize);
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
        category.Should().NotBeNull();
        category.Id.Should().NotBeNullOrEmpty();
        category.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MemeCategory_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var category = MemeTestDataFactory.CreateSampleCategory();

        // Assert
        category.IsActive.Should().BeTrue();
        category.DisplayOrder.Should().BeGreaterThanOrEqualTo(0);
        category.Description.Should().NotBeNull();
        category.Color.Should().NotBeNullOrEmpty();
        category.Icon.Should().NotBeNull();
        category.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromDays(90));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MemeCategory_IsActive_ShouldReflectProvidedValue(bool isActive)
    {
        // Arrange & Act
        var category = MemeTestDataFactory.CreateSampleCategory(isActive: isActive);

        // Assert
        category.IsActive.Should().Be(isActive);
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
        category.DisplayOrder.Should().Be(displayOrder);
    }

    [Fact]
    public void MemeCategory_MultipleCategories_ShouldBeUnique()
    {
        // Arrange & Act
        var categories = MemeTestDataFactory.CreateMultipleCategories();

        // Assert
        categories.Should().NotBeEmpty();
        categories.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        categories.Select(c => c.Name).Should().OnlyHaveUniqueItems();
    }
}
