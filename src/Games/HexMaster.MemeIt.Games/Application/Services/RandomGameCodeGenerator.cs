using System;
using System.Security.Cryptography;
using HexMaster.MemeIt.Games.Abstractions.Services;

namespace HexMaster.MemeIt.Games.Application.Services;

/// <summary>
/// Generates random alpha-numeric codes for new games.
/// </summary>
public sealed class RandomGameCodeGenerator : IGameCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789"; // omit confusing chars
    private const int CodeLength = 8;

    public string Generate()
    {
        var buffer = new char[CodeLength];
        var bytes = RandomNumberGenerator.GetBytes(CodeLength);

        for (var i = 0; i < CodeLength; i++)
        {
            var index = bytes[i] % Alphabet.Length;
            buffer[i] = Alphabet[index];
        }

        return new string(buffer);
    }
}
