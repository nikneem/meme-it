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
        Assert.NotNull(document);
        Assert.Equal(meme.Id, document.Id);
        Assert.Equal(meme.Name, document.Name);
        Assert.Equal(meme.ImageUrl, document.ImageUrl);
        Assert.Equal(meme.Categories.Count, document.Categories.Count);
        Assert.Equal(meme.Tags.Count, document.Tags.Count);
        Assert.Equal(meme.Width, document.Width);
        Assert.Equal(meme.Height, document.Height);
        Assert.Equal(meme.IsActive, document.IsActive);
        Assert.Equal(meme.DifficultyLevel, document.DifficultyLevel);
        Assert.Equal(meme.PopularityScore, document.PopularityScore);
        Assert.Equal("meme", document.Type);
        Assert.Equal(meme.TextAreas.Count, document.TextAreas.Count);
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
        Assert.NotNull(domainMeme);
        Assert.Equal(originalMeme.Id, domainMeme.Id);
        Assert.Equal(originalMeme.Name, domainMeme.Name);
        Assert.Equal(originalMeme.ImageUrl, domainMeme.ImageUrl);
        Assert.Equal(originalMeme.Width, domainMeme.Width);
        Assert.Equal(originalMeme.Height, domainMeme.Height);
        Assert.Equal(originalMeme.IsActive, domainMeme.IsActive);
        Assert.Equal(originalMeme.DifficultyLevel, domainMeme.DifficultyLevel);
        Assert.Equal(originalMeme.PopularityScore, domainMeme.PopularityScore);
    }

    [Fact]
    public void MemeDocument_PartitionKey_ShouldBeFirstCategory()
    {
        // Arrange
        var meme = MemeTestDataFactory.CreateSampleMeme(categories: new List<string> { "humor", "classic" });

        // Act
        var document = MemeDocument.FromDomain(meme);

        // Assert
        Assert.Equal("humor", document.PartitionKey);
    }

    [Fact]
    public void MemeDocument_PartitionKey_ShouldBeDefaultWhenNoCategories()
    {
        // Arrange
        var meme = MemeTestDataFactory.CreateSampleMeme(categories: new List<string>());

        // Act
        var document = MemeDocument.FromDomain(meme);

        // Assert
        Assert.Equal("default", document.PartitionKey);
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
        Assert.Equal(originalMeme.Id, resultMeme.Id);
        Assert.Equal(originalMeme.Name, resultMeme.Name);
        Assert.Equal(originalMeme.ImageUrl, resultMeme.ImageUrl);
        Assert.Equal(originalMeme.Width, resultMeme.Width);
        Assert.Equal(originalMeme.Height, resultMeme.Height);
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
        Assert.NotNull(document);
        Assert.Equal(textArea.Id, document.Id);
        Assert.Equal(textArea.X, document.X);
        Assert.Equal(textArea.Y, document.Y);
        Assert.Equal(textArea.Width, document.Width);
        Assert.Equal(textArea.Height, document.Height);
        Assert.Equal(textArea.FontSize, document.FontSize);
        Assert.Equal(textArea.MaxCharacters, document.MaxCharacters);
        Assert.Equal(textArea.Alignment.ToString(), document.Alignment);
        Assert.Equal(textArea.FontColor, document.FontColor);
        Assert.Equal(textArea.HasStroke, document.HasStroke);
        Assert.Equal(textArea.StrokeColor, document.StrokeColor);
        Assert.Equal(textArea.StrokeWidth, document.StrokeWidth);
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
        Assert.NotNull(domainTextArea);
        Assert.Equal(originalTextArea.Id, domainTextArea.Id);
        Assert.Equal(originalTextArea.X, domainTextArea.X);
        Assert.Equal(originalTextArea.Y, domainTextArea.Y);
        Assert.Equal(originalTextArea.Width, domainTextArea.Width);
        Assert.Equal(originalTextArea.Height, domainTextArea.Height);
        Assert.Equal(originalTextArea.FontSize, domainTextArea.FontSize);
        Assert.Equal(originalTextArea.MaxCharacters, domainTextArea.MaxCharacters);
        Assert.Equal(originalTextArea.Alignment, domainTextArea.Alignment);
        Assert.Equal(originalTextArea.FontColor, domainTextArea.FontColor);
        Assert.Equal(originalTextArea.HasStroke, domainTextArea.HasStroke);
        Assert.Equal(originalTextArea.StrokeColor, domainTextArea.StrokeColor);
        Assert.Equal(originalTextArea.StrokeWidth, domainTextArea.StrokeWidth);
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
        Assert.Equal(originalTextArea.Id, resultTextArea.Id);
        Assert.Equal(originalTextArea.X, resultTextArea.X);
        Assert.Equal(originalTextArea.Y, resultTextArea.Y);
        Assert.Equal(originalTextArea.Width, resultTextArea.Width);
        Assert.Equal(originalTextArea.Height, resultTextArea.Height);
        Assert.Equal(originalTextArea.FontSize, resultTextArea.FontSize);
        Assert.Equal(originalTextArea.MaxCharacters, resultTextArea.MaxCharacters);
        Assert.Equal(originalTextArea.Alignment, resultTextArea.Alignment);
        Assert.Equal(originalTextArea.FontColor, resultTextArea.FontColor);
        Assert.Equal(originalTextArea.HasStroke, resultTextArea.HasStroke);
        Assert.Equal(originalTextArea.StrokeColor, resultTextArea.StrokeColor);
        Assert.Equal(originalTextArea.StrokeWidth, resultTextArea.StrokeWidth);
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
        Assert.NotNull(document);
        Assert.Equal(category.Id, document.Id);
        Assert.Equal(category.Name, document.Name);
        Assert.Equal(category.Description, document.Description);
        Assert.Equal(category.IsActive, document.IsActive);
        Assert.Equal(category.DisplayOrder, document.DisplayOrder);
        Assert.Equal(category.Color, document.Color);
        Assert.Equal(category.Icon, document.Icon);
        Assert.Equal("category", document.PartitionKey);
        Assert.Equal("category", document.Type);
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
        Assert.NotNull(domainCategory);
        Assert.Equal(originalCategory.Id, domainCategory.Id);
        Assert.Equal(originalCategory.Name, domainCategory.Name);
        Assert.Equal(originalCategory.Description, domainCategory.Description);
        Assert.Equal(originalCategory.IsActive, domainCategory.IsActive);
        Assert.Equal(originalCategory.DisplayOrder, domainCategory.DisplayOrder);
        Assert.Equal(originalCategory.Color, domainCategory.Color);
        Assert.Equal(originalCategory.Icon, domainCategory.Icon);
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
        Assert.Equal(originalCategory.Id, resultCategory.Id);
        Assert.Equal(originalCategory.Name, resultCategory.Name);
        Assert.Equal(originalCategory.Description, resultCategory.Description);
        Assert.Equal(originalCategory.IsActive, resultCategory.IsActive);
        Assert.Equal(originalCategory.DisplayOrder, resultCategory.DisplayOrder);
        Assert.Equal(originalCategory.Color, resultCategory.Color);
        Assert.Equal(originalCategory.Icon, resultCategory.Icon);
    }

    [Fact]
    public void MemeCategoryDocument_PartitionKey_ShouldBeCategory()
    {
        // Arrange
        var category = MemeTestDataFactory.CreateSampleCategory();

        // Act
        var document = MemeCategoryDocument.FromDomain(category);

        // Assert
        Assert.Equal("category", document.PartitionKey);
    }
}
