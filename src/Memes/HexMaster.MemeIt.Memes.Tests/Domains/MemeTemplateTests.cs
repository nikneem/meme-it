using Bogus;
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
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act
        var template = MemeTemplate.Create(title, imageUrl, 800, 600, textAreas);

        // Assert
        Assert.NotNull(template);
        Assert.NotEqual(Guid.Empty, template.Id);
        Assert.Equal(title, template.Title);
        Assert.Equal(imageUrl, template.ImageUrl);
        Assert.Single(template.TextAreas);
        Assert.True((DateTimeOffset.UtcNow - template.CreatedAt).Duration() < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowException()
    {
        // Arrange
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MemeTemplate.Create("", imageUrl, 800, 600, textAreas));
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
            MemeTemplate.Create(title, "not-a-valid-url", 800, 600, textAreas));
        Assert.Contains("relative path starting with /", exception.Message);
    }

    [Fact]
    public void Create_WithNoTextAreas_ShouldThrowDomainException()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var textAreas = new List<TextAreaDefinition>();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            MemeTemplate.Create(title, imageUrl, 800, 600, textAreas));
        Assert.Contains("At least one text area", exception.Message);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateTemplate()
    {
        // Arrange
        var template = CreateValidTemplate();
        var newTitle = _faker.Lorem.Sentence();
        var newImageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var newTextAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(20, 20, 300, 60, 32, "#FF0000", 3, "#FFFFFF", false)
        };

        // Act
        template.Update(newTitle, newImageUrl, 800, 600, newTextAreas);

        // Assert
        Assert.Equal(newTitle, template.Title);
        Assert.Equal(newImageUrl, template.ImageUrl);
        Assert.Single(template.TextAreas);
        Assert.NotNull(template.UpdatedAt);
        Assert.True((DateTimeOffset.UtcNow - template.UpdatedAt.Value).Duration() < TimeSpan.FromSeconds(5));
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
        Assert.Equal(2, template.TextAreas.Count);
        Assert.NotNull(template.UpdatedAt);
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
        Assert.Single(template.TextAreas);
    }

    [Fact]
    public void RemoveTextArea_WhenLastTextArea_ShouldThrowDomainException()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => template.RemoveTextArea(0));
        Assert.Contains("Cannot remove the last text area", exception.Message);
    }

    [Fact]
    public void RemoveTextArea_WithInvalidIndex_ShouldThrowDomainException()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => template.RemoveTextArea(10));
        Assert.Contains("Invalid text area index", exception.Message);
    }

    [Fact]
    public void Create_WithZeroWidth_ShouldThrowDomainException()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            MemeTemplate.Create(title, imageUrl, 0, 600, textAreas));
        Assert.Contains("Width must be greater than 0", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeWidth_ShouldThrowDomainException()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            MemeTemplate.Create(title, imageUrl, -100, 600, textAreas));
        Assert.Contains("Width must be greater than 0", exception.Message);
    }

    [Fact]
    public void Create_WithZeroHeight_ShouldThrowDomainException()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            MemeTemplate.Create(title, imageUrl, 800, 0, textAreas));
        Assert.Contains("Height must be greater than 0", exception.Message);
    }

    [Fact]
    public void Create_WithNullTextAreas_ShouldThrowArgumentNullException()
    {
        // Arrange
        var title = _faker.Lorem.Sentence();
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            MemeTemplate.Create(title, imageUrl, 800, 600, null!));
    }

    [Fact]
    public void AddTextArea_WithNullTextArea_ShouldThrowArgumentNullException()
    {
        // Arrange
        var template = CreateValidTemplate();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => template.AddTextArea(null!));
    }

    [Fact]
    public void Update_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Arrange
        var template = CreateValidTemplate();
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            template.Update(null!, "/test.jpg", 800, 600, textAreas));
    }

    [Fact]
    public void Update_WithZeroWidth_ShouldThrowDomainException()
    {
        // Arrange
        var template = CreateValidTemplate();
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            template.Update("New Title", "/test.jpg", 0, 600, textAreas));
        Assert.Contains("Width must be greater than 0", exception.Message);
    }

    [Fact]
    public void RemoveTextArea_WithNegativeIndex_ShouldThrowDomainException()
    {
        // Arrange
        var template = CreateValidTemplate();
        template.AddTextArea(TextAreaDefinition.Create(50, 50, 150, 40, 18, "#00FF00", 1, "#000000", true));

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => template.RemoveTextArea(-1));
        Assert.Contains("Invalid text area index", exception.Message);
    }

    private MemeTemplate CreateValidTemplate()
    {
        var title = _faker.Lorem.Sentence();
        var imageUrl = "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg";
        var textAreas = new List<TextAreaDefinition>
        {
            TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
        };
        return MemeTemplate.Create(title, imageUrl, 800, 600, textAreas);
    }
}
