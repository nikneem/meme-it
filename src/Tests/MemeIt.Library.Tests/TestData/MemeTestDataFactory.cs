using MemeIt.Core.Models;

namespace MemeIt.Library.Tests.TestData;

/// <summary>
/// Test data factory for creating memes and related objects
/// </summary>
public static class MemeTestDataFactory
{
    public static Meme CreateSampleMeme(
        string id = "test-meme-1",
        string name = "Test Meme",
        string imageUrl = "https://example.com/meme.jpg",
        IReadOnlyList<string>? categories = null,
        IReadOnlyList<TextArea>? textAreas = null,
        bool isActive = true,
        int popularityScore = 0)
    {
        return new Meme
        {
            Id = id,
            Name = name,
            ImageUrl = imageUrl,
            Categories = categories ?? ["humor", "classic"],
            TextAreas = textAreas ?? CreateSampleTextAreas(),
            Width = 800,
            Height = 600,
            IsActive = isActive,
            PopularityScore = popularityScore,
            DifficultyLevel = 1,
            Tags = ["funny", "classic"],
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-30),
            ModifiedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
    }

    public static List<Meme> CreateMultipleMemes(int count = 5)
    {
        var memes = new List<Meme>();
        var categories = new string[][] { new[] { "humor" }, new[] { "reactions" }, new[] { "animals" }, new[] { "classic" } };
        
        for (int i = 0; i < count; i++)
        {
            memes.Add(CreateSampleMeme(
                id: $"meme-{i + 1}",
                name: $"Test Meme {i + 1}",
                imageUrl: $"https://example.com/meme{i + 1}.jpg",
                categories: categories[i % categories.Length],
                popularityScore: i * 10
            ));
        }

        return memes;
    }

    public static MemeCategory CreateSampleCategory(
        string id = "humor",
        string name = "Humor",
        bool isActive = true,
        int displayOrder = 0)
    {
        return new MemeCategory
        {
            Id = id,
            Name = name,
            Description = $"{name} category for testing",
            IsActive = isActive,
            DisplayOrder = displayOrder,
            Color = "#FF5733",
            Icon = "😄",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-60)
        };
    }

    public static List<MemeCategory> CreateMultipleCategories()
    {
        return new List<MemeCategory>
        {
            CreateSampleCategory("humor", "Humor", true, 1),
            CreateSampleCategory("reactions", "Reactions", true, 2),
            CreateSampleCategory("animals", "Animals", true, 3),
            CreateSampleCategory("classic", "Classic", true, 4),
            CreateSampleCategory("inactive", "Inactive Category", false, 5)
        };
    }

    public static List<TextArea> CreateSampleTextAreas()
    {
        return new List<TextArea>
        {
            new TextArea
            {
                Id = "top-text",
                X = 10,
                Y = 10,
                Width = 780,
                Height = 100,
                FontSize = 48,
                MaxCharacters = 50,
                Alignment = TextAlignment.Center,
                FontColor = "#FFFFFF",
                HasStroke = true,
                StrokeColor = "#000000",
                StrokeWidth = 2
            },
            new TextArea
            {
                Id = "bottom-text",
                X = 10,
                Y = 490,
                Width = 780,
                Height = 100,
                FontSize = 48,
                MaxCharacters = 50,
                Alignment = TextAlignment.Center,
                FontColor = "#FFFFFF",
                HasStroke = true,
                StrokeColor = "#000000",
                StrokeWidth = 2
            }
        };
    }

    public static TextArea CreateSampleTextArea(
        string id = "text-area-1",
        int x = 10,
        int y = 10,
        int width = 200,
        int height = 50,
        int fontSize = 24)
    {
        return new TextArea
        {
            Id = id,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            FontSize = fontSize,
            MaxCharacters = 100,
            Alignment = TextAlignment.Center,
            FontColor = "#FFFFFF",
            HasStroke = true,
            StrokeColor = "#000000",
            StrokeWidth = 2
        };
    }
}
