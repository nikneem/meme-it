using Orleans;

namespace MemeIt.Api.Orleans.Interfaces;

public interface IMemeCounterGrain : IGrainWithStringKey
{
    ValueTask<int> GetCountAsync();
    ValueTask<int> IncrementAsync();
    ValueTask ResetAsync();
}
