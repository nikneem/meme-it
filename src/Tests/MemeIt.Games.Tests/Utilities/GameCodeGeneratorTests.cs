using MemeIt.Games.Utilities;

namespace MemeIt.Games.Tests.Utilities;

public class GameCodeGeneratorTests
{
    [Fact]
    public void GenerateGameCode_ShouldReturn6CharacterCode()
    {
        // Act
        var gameCode = GameCodeGenerator.GenerateGameCode();

        // Assert
        Assert.Equal(6, gameCode.Length);
        Assert.All(gameCode, c => Assert.True(char.IsLetterOrDigit(c) && (char.IsDigit(c) || char.IsUpper(c))));
    }

    [Fact]
    public void GenerateGameCode_ShouldReturnUniqueCodesMostOfTheTime()
    {
        // Act
        var codes = new HashSet<string>();
        for (int i = 0; i < 100; i++)
        {
            codes.Add(GameCodeGenerator.GenerateGameCode());
        }

        // Assert - Should have high uniqueness (allowing some duplicates due to randomness)
        Assert.True(codes.Count > 90, $"Expected more than 90 unique codes, got {codes.Count}");
    }

    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("ABCDEF", true)]
    [InlineData("123456", true)]
    [InlineData("A1B2C3", true)]
    [InlineData("ZZZZZZ", true)]
    [InlineData("000000", true)]
    [InlineData("abc123", false)] // lowercase
    [InlineData("ABC12", false)]  // too short
    [InlineData("ABC1234", false)] // too long
    [InlineData("ABC12!", false)]  // special character
    [InlineData("ABC 12", false)]  // space
    [InlineData("ABC-12", false)]  // dash
    [InlineData("", false)]        // empty
    [InlineData(" ", false)]       // whitespace
    public void IsValidGameCode_ShouldValidateCorrectly(string gameCode, bool expected)
    {
        // Act
        var result = GameCodeGenerator.IsValidGameCode(gameCode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidGameCode_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = GameCodeGenerator.IsValidGameCode(null!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("abc123", "ABC123")]
    [InlineData("AbC123", "ABC123")]
    [InlineData("ABC123", "ABC123")]
    [InlineData("xyz789", "XYZ789")]
    [InlineData("a1b2c3", "A1B2C3")]
    [InlineData("abc12", null)]   // too short
    [InlineData("abc1234", null)] // too long
    [InlineData("abc12!", null)]  // invalid character
    [InlineData("abc 12", null)]  // space
    [InlineData("", null)]        // empty
    [InlineData(" ", null)]       // whitespace only
    public void NormalizeGameCode_ShouldNormalizeCorrectly(string input, string? expected)
    {
        // Act
        var result = GameCodeGenerator.NormalizeGameCode(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeGameCode_WithNull_ShouldReturnNull()
    {
        // Act
        var result = GameCodeGenerator.NormalizeGameCode(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateGameCode_ShouldGenerateOnlyValidCodes()
    {
        // Act & Assert
        for (int i = 0; i < 50; i++)
        {
            var code = GameCodeGenerator.GenerateGameCode();
            Assert.True(GameCodeGenerator.IsValidGameCode(code), $"Generated invalid code: {code}");
        }
    }

    [Fact]
    public void NormalizeGameCode_ShouldPreserveValidCodes()
    {
        // Arrange
        var validCodes = new[] { "ABC123", "XYZ789", "A1B2C3", "000000", "ZZZZZZ" };

        // Act & Assert
        foreach (var code in validCodes)
        {
            var normalized = GameCodeGenerator.NormalizeGameCode(code);
            Assert.Equal(code, normalized);
        }
    }
}
