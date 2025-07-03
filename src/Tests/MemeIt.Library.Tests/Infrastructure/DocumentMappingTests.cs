using FluentAssertions;
using MemeIt.Library.Infrastructure.Models;
using MemeIt.Library.Tests.TestData;

namespace MemeIt.Library.Tests.Infrastructure;

public class MemeDocumentTests
{
    [Fact]
    public void MemeDocument_FromDomain_ShouldMapCorrectly()
    {
        // Arrange
        var meme = MemeTestDataFactory.CreateSampleMeme();

        // Act
        var document = MemeDocument.FromDomain(meme);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().Be(meme.Id);
        document.Name.Should().Be(meme.Name);
        document.ImageUrl.Should().Be(meme.ImageUrl);
        document.Categories.Should().BeEquivalentTo(meme.Categories);
        document.Tags.Should().BeEquivalentTo(meme.Tags);
        document.Width.Should().Be(meme.Width);
        document.Height.Should().Be(meme.Height);
        document.IsActive.Should().Be(meme.IsActive);
        document.DifficultyLevel.Should().Be(meme.DifficultyLevel);
        document.PopularityScore.Should().Be(meme.PopularityScore);
        document.Type.Should().Be("meme");
        document.TextAreas.Should().HaveCount(meme.TextAreas.Count);
    }

    [Fact]
    public void MemeDocument_ToDomain_ShouldMapCorrectly()
    {
        // Arrange
        var originalMeme = MemeTestDataFactory.CreateSampleMeme();
        var document = MemeDocument.FromDomain(originalMeme);

        // Act
        var domainMeme = document.ToDomain();

        // Assert
        domainMeme.Should().NotBeNull();
        domainMeme.Should().BeEquivalentTo(originalMeme);
    }

    [Fact]
    public void MemeDocument_PartitionKey_ShouldBeFirstCategory()
    {
        // Arrange
        var categories = new List<string> { "humor", "classic", "animals" };
        var meme = MemeTestDataFactory.CreateSampleMeme(categories: categories);

        // Act
        var document = MemeDocument.FromDomain(meme);

        // Assert
        document.PartitionKey.Should().Be("humor");
    }

    [Fact]
    public void MemeDocument_PartitionKey_WithEmptyCategories_ShouldBeDefault()
    {
        // Arrange
        var meme = MemeTestDataFactory.CreateSampleMeme(categories: new List<string>());

        // Act
        var document = MemeDocument.FromDomain(meme);

        // Assert
        document.PartitionKey.Should().Be("default");
    }

    [Fact]
    public void MemeDocument_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalMeme = MemeTestDataFactory.CreateSampleMeme();

        // Act
        var document = MemeDocument.FromDomain(originalMeme);
        var resultMeme = document.ToDomain();

        // Assert
        resultMeme.Should().BeEquivalentTo(originalMeme);
    }
}

public class TextAreaDocumentTests
{
    [Fact]
    public void TextAreaDocument_FromDomain_ShouldMapCorrectly()
    {
        // Arrange
        var textArea = MemeTestDataFactory.CreateSampleTextArea();

        // Act
        var document = TextAreaDocument.FromDomain(textArea);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().Be(textArea.Id);
        document.X.Should().Be(textArea.X);
        document.Y.Should().Be(textArea.Y);
        document.Width.Should().Be(textArea.Width);
        document.Height.Should().Be(textArea.Height);
        document.FontSize.Should().Be(textArea.FontSize);
        document.MaxCharacters.Should().Be(textArea.MaxCharacters);
        document.Alignment.Should().Be(textArea.Alignment.ToString());
        document.FontColor.Should().Be(textArea.FontColor);
        document.HasStroke.Should().Be(textArea.HasStroke);
        document.StrokeColor.Should().Be(textArea.StrokeColor);
        document.StrokeWidth.Should().Be(textArea.StrokeWidth);
    }

    [Fact]
    public void TextAreaDocument_ToDomain_ShouldMapCorrectly()
    {
        // Arrange
        var originalTextArea = MemeTestDataFactory.CreateSampleTextArea();
        var document = TextAreaDocument.FromDomain(originalTextArea);

        // Act
        var domainTextArea = document.ToDomain();

        // Assert
        domainTextArea.Should().NotBeNull();
        domainTextArea.Should().BeEquivalentTo(originalTextArea);
    }

    [Fact]
    public void TextAreaDocument_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalTextArea = MemeTestDataFactory.CreateSampleTextArea();

        // Act
        var document = TextAreaDocument.FromDomain(originalTextArea);
        var resultTextArea = document.ToDomain();

        // Assert
        resultTextArea.Should().BeEquivalentTo(originalTextArea);
    }
}

public class MemeCategoryDocumentTests
{
    [Fact]
    public void MemeCategoryDocument_FromDomain_ShouldMapCorrectly()
    {
        // Arrange
        var category = MemeTestDataFactory.CreateSampleCategory();

        // Act
        var document = MemeCategoryDocument.FromDomain(category);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().Be(category.Id);
        document.Name.Should().Be(category.Name);
        document.Description.Should().Be(category.Description);
        document.IsActive.Should().Be(category.IsActive);
        document.DisplayOrder.Should().Be(category.DisplayOrder);
        document.Color.Should().Be(category.Color);
        document.Icon.Should().Be(category.Icon);
        document.PartitionKey.Should().Be("category");
        document.Type.Should().Be("category");
    }

    [Fact]
    public void MemeCategoryDocument_ToDomain_ShouldMapCorrectly()
    {
        // Arrange
        var originalCategory = MemeTestDataFactory.CreateSampleCategory();
        var document = MemeCategoryDocument.FromDomain(originalCategory);

        // Act
        var domainCategory = document.ToDomain();

        // Assert
        domainCategory.Should().NotBeNull();
        domainCategory.Should().BeEquivalentTo(originalCategory);
    }

    [Fact]
    public void MemeCategoryDocument_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalCategory = MemeTestDataFactory.CreateSampleCategory();

        // Act
        var document = MemeCategoryDocument.FromDomain(originalCategory);
        var resultCategory = document.ToDomain();

        // Assert
        resultCategory.Should().BeEquivalentTo(originalCategory);
    }

    [Fact]
    public void MemeCategoryDocument_PartitionKey_ShouldAlwaysBeCategory()
    {
        // Arrange
        var category = MemeTestDataFactory.CreateSampleCategory();

        // Act
        var document = MemeCategoryDocument.FromDomain(category);

        // Assert
        document.PartitionKey.Should().Be("category");
    }
}
