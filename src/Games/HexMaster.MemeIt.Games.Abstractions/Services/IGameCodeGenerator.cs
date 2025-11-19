namespace HexMaster.MemeIt.Games.Abstractions.Services;

/// <summary>
/// Abstraction for generating unique lobby codes for new games.
/// </summary>
public interface IGameCodeGenerator
{
    /// <summary>
    /// Generates a new eight-character game code.
    /// </summary>
    /// <returns>Uppercase alpha-numeric code.</returns>
    string Generate();
}
