using Bogus;
using FluentAssertions;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;

namespace HexMaster.MemeIt.Memes.Tests.Domains;

public class MemeTemplateTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Create_WithValidData_ShouldCreateTemplate()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var imageUrl = _faker.Internet.UrlWithPath();
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act
        var template = MemeTemplate.Create(title, imageUrl, textAreas);

        // Assert
        template.Should().NotBeNull();
        template.Id.Should().NotBeEmpty();
        template.Title.Should().Be(title);
        template.ImageUrl.Should().Be(imageUrl);
        template.TextAreas.Should().HaveCount(1);
        template.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowException()
    {
        // Arrange
        var imageUrl = _faker.Internet.UrlWithPath();
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MemeTemplate.Create("", imageUrl, textAreas));
    }

    [Fact]
    public void Create_WithInvalidImageUrl_ShouldThrowDomainException()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            MemeTemplate.Create(title, "not-a-valid-url", textAreas));
        exception.Message.Should().Contain("valid absolute URI");
    }

    [Fact]
    public void Create_WithNoTextAreas_ShouldThrowDomainException()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var imageUrl = _faker.Internet.UrlWithPath();
        var textAreas = new List<TextAreaDefinition>();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            MemeTemplate.Create(title, imageUrl, textAreas));
        exception.Message.Should().Contain("At least one text area");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateTemplate()
    {
        // Arrange
        var template = CreateValidTemplate();
        var newTitle = _faker.Lorem.Sentence();
        var newImageUrl = _faker.Internet.UrlWithPath();
        var newTextAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(20, 20, 300, 60, 32, "#FF0000", 3, "#FFFFFF", false)
        };

        // Act
        template.Update(newTitle, newImageUrl, newTextAreas);

        // Assert
        template.Title.Should().Be(newTitle);
        template.ImageUrl.Should().Be(newImageUrl);
        template.TextAreas.Should().HaveCount(1);
        template.UpdatedAt.Should().NotBeNull();
        template.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddTextArea_WithValidTextArea_ShouldAddToCollection()
    {
        // Arrange
        var template = CreateValidTemplate();
        var newTextArea = TextAreaDefinition.Create(50, 50, 150, 40, 18, "#00FF00", 1, "#000000", true);

        // Act
        template.AddTextArea(newTextArea);

        // Assert
        template.TextAreas.Should().HaveCount(2);
        template.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemoveTextArea_WithValidIndex_ShouldRemoveFromCollection()
    {
        // Arrange
        var template = CreateValidTemplate();
        template.AddTextArea(TextAreaDefinition.Create(50, 50, 150, 40, 18, "#00FF00", 1, "#000000", true));

        // Act
        template.RemoveTextArea(0);

        // Assert
        template.TextAreas.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveTextArea_WhenLastTextArea_ShouldThrowDomainException()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => template.RemoveTextArea(0));
        exception.Message.Should().Contain("Cannot remove the last text area");
    }

    [Fact]
    public void RemoveTextArea_WithInvalidIndex_ShouldThrowDomainException()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => template.RemoveTextArea(10));
        exception.Message.Should().Contain("Invalid text area index");
    }

    private MemeTemplate CreateValidTemplate()
    {
        var title = _faker.Lorem.Sentence();
        var imageUrl = _faker.Internet.UrlWithPath();
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };
        return MemeTemplate.Create(title, imageUrl, textAreas);
    }
}
