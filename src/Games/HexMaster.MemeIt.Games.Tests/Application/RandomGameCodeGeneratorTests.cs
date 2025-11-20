using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Services;

namespace HexMaster.MemeIt.Games.Tests.Application;

public class RandomGameCodeGeneratorTests
{
    private readonly IGameCodeGenerator _generator;

    public RandomGameCodeGeneratorTests()
    {
        _generator = new RandomGameCodeGenerator();
    }

    [Fact]
    public void Generate_ShouldReturnEightCharacterCode()
    {
        // Act
        var code = _generator.Generate();

        // Assert
        Assert.NotNull(code);
        Assert.Equal(8, code.Length);
    }

    [Fact]
    public void Generate_ShouldReturnDifferentCodes()
    {
        // Act
        var code1 = _generator.Generate();
        var code2 = _generator.Generate();

        // Assert
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void Generate_ShouldOnlyContainValidCharacters()
    {
        // Arrange
        var validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789";

        // Act
        var code = _generator.Generate();

        // Assert
        Assert.All(code, c => Assert.Contains(c, validChars));
    }

    [Fact]
    public void Generate_ShouldBeUpperCase()
    {
        // Act
        var code = _generator.Generate();

        // Assert
        Assert.Equal(code, code.ToUpper());
    }

    [Fact]
    public void Generate_CalledMultipleTimes_ShouldProduceUniqueResults()
    {
        // Arrange
        var codes = new HashSet<string>();
        var iterations = 100;

        // Act
        for (var i = 0; i < iterations; i++)
        {
            codes.Add(_generator.Generate());
        }

        // Assert
        // Should have high uniqueness (allow for small collision possibility)
        Assert.True(codes.Count >= iterations * 0.95);
    }
}
