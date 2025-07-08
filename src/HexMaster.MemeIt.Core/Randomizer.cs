namespace HexMaster.MemeIt.Core;

public static class Randomizer
{
    
    private const string Pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly Random Random = new Random();


    private static string GenerateRandomString(int length)
    {
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = Pool[Random.Next(Pool.Length)];
        }
        return new string(result);
    }

    public static string GenerateGameCode()
    {
        return GenerateRandomString(6);
    }

}