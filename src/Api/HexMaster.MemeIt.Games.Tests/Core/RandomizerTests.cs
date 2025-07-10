using HexMaster.MemeIt.Core;

namespace HexMaster.MemeIt.Games.Tests.Core;

public class RandomizerTests
{
    [Fact]
    public void GenerateGameCode_ReturnsNonEmptyString()
    {
        // Act
        var gameCode = Randomizer.GenerateGameCode();

        // Assert
        Assert.NotNull(gameCode);
        Assert.NotEmpty(gameCode);
    }

    [Fact]
    public void GenerateGameCode_ReturnsStringOfLength6()
    {
        // Act
        var gameCode = Randomizer.GenerateGameCode();

        // Assert
        Assert.Equal(6, gameCode.Length);
    }

    [Fact]
    public void GenerateGameCode_ReturnsOnlyAlphanumericCharacters()
    {
        // Act
        var gameCode = Randomizer.GenerateGameCode();

        // Assert
        Assert.True(gameCode.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void GenerateGameCode_ReturnsOnlyUppercaseAndDigits()
    {
        // Act
        var gameCode = Randomizer.GenerateGameCode();

        // Assert
        Assert.True(gameCode.All(c => char.IsUpper(c) || char.IsDigit(c)));
    }

    [Fact]
    public void GenerateGameCode_MultipleCallsReturnDifferentValues()
    {
        // Act
        var gameCode1 = Randomizer.GenerateGameCode();
        var gameCode2 = Randomizer.GenerateGameCode();
        var gameCode3 = Randomizer.GenerateGameCode();

        // Assert
        // While theoretically possible to get duplicates, it's extremely unlikely with 6 characters
        Assert.NotEqual(gameCode1, gameCode2);
        Assert.NotEqual(gameCode2, gameCode3);
        Assert.NotEqual(gameCode1, gameCode3);
    }

    [Fact]
    public void GenerateGameCode_UsesExpectedCharacterPool()
    {
        // Arrange
        const string expectedPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var generatedCodes = new List<string>();

        // Act - Generate multiple codes to get a good sample
        for (int i = 0; i < 100; i++)
        {
            generatedCodes.Add(Randomizer.GenerateGameCode());
        }

        // Assert
        var allCharactersUsed = string.Join("", generatedCodes);
        Assert.True(allCharactersUsed.All(c => expectedPool.Contains(c)));
    }

    [Fact]
    public void GenerateGameCode_RepeatedCalls_ProduceVariedResults()
    {
        // Arrange
        var gameCodes = new HashSet<string>();
        const int numberOfCodes = 1000;

        // Act
        for (int i = 0; i < numberOfCodes; i++)
        {
            gameCodes.Add(Randomizer.GenerateGameCode());
        }

        // Assert
        // With 6 characters from a 36-character pool, we should get very few duplicates
        // Expecting at least 95% unique codes
        Assert.True(gameCodes.Count > numberOfCodes * 0.95);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void GenerateGameCode_MultipleGenerations_AllHaveCorrectLength(int numberOfGenerations)
    {
        // Act & Assert
        for (int i = 0; i < numberOfGenerations; i++)
        {
            var gameCode = Randomizer.GenerateGameCode();
            Assert.Equal(6, gameCode.Length);
        }
    }

    [Fact]
    public void GenerateGameCode_DoesNotContainLowercaseLetters()
    {
        // Arrange
        var gameCodes = new List<string>();

        // Act - Generate multiple codes to test thoroughly
        for (int i = 0; i < 100; i++)
        {
            gameCodes.Add(Randomizer.GenerateGameCode());
        }

        // Assert
        var allCharacters = string.Join("", gameCodes);
        Assert.True(allCharacters.All(c => !char.IsLower(c)));
    }

    [Fact]
    public void GenerateGameCode_DoesNotContainSpecialCharacters()
    {
        // Arrange
        var gameCodes = new List<string>();

        // Act - Generate multiple codes to test thoroughly
        for (int i = 0; i < 100; i++)
        {
            gameCodes.Add(Randomizer.GenerateGameCode());
        }

        // Assert
        var allCharacters = string.Join("", gameCodes);
        Assert.True(allCharacters.All(c => char.IsLetterOrDigit(c)));
    }
}
