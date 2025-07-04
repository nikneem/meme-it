namespace MemeIt.Games.Utilities;

public static class GameCodeGenerator
{
    private static readonly char[] AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    private static readonly Random Random = new();
    
    /// <summary>
    /// Generates a unique 6-character alphanumeric game code in uppercase
    /// </summary>
    /// <returns>A 6-character game code</returns>
    public static string GenerateGameCode()
    {
        var code = new char[6];
        for (int i = 0; i < 6; i++)
        {
            code[i] = AllowedCharacters[Random.Next(AllowedCharacters.Length)];
        }
        return new string(code);
    }
    
    /// <summary>
    /// Validates if a game code has the correct format (6 alphanumeric uppercase characters)
    /// </summary>
    /// <param name="gameCode">The game code to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidGameCode(string gameCode)
    {
        if (string.IsNullOrEmpty(gameCode) || gameCode.Length != 6)
            return false;
            
        return gameCode.All(c => AllowedCharacters.Contains(c));
    }
    
    /// <summary>
    /// Normalizes a game code to uppercase and validates it
    /// </summary>
    /// <param name="gameCode">The game code to normalize</param>
    /// <returns>The normalized game code or null if invalid</returns>
    public static string? NormalizeGameCode(string? gameCode)
    {
        if (string.IsNullOrEmpty(gameCode))
            return null;
            
        var normalized = gameCode.ToUpperInvariant();
        return IsValidGameCode(normalized) ? normalized : null;
    }
}
