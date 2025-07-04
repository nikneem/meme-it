using Orleans;
using MemeIt.Core.Models;

namespace MemeIt.Games.Abstractions.Grains;

public interface IMemeServiceGrain : IGrainWithIntegerKey
{
    Task<List<Meme>> GetRandomMemesAsync(int count, List<string>? categories = null);
    Task<Meme?> GetMemeAsync(string memeId);
    Task<List<Meme>> GetMemesByCategoryAsync(string category, int count = 10);
}
