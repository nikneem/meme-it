using Orleans;
using Orleans.Runtime;
using MemeIt.Api.Orleans.Interfaces;

namespace MemeIt.Api.Orleans.Grains;

public class MemeCounterGrain : Grain, IMemeCounterGrain
{
    private readonly IPersistentState<CounterState> _state;

    public MemeCounterGrain([PersistentState("counter", "Default")] IPersistentState<CounterState> state)
    {
        _state = state;
    }

    public ValueTask<int> GetCountAsync()
    {
        return ValueTask.FromResult(_state.State.Count);
    }

    public async ValueTask<int> IncrementAsync()
    {
        _state.State.Count++;
        await _state.WriteStateAsync();
        return _state.State.Count;
    }

    public async ValueTask ResetAsync()
    {
        _state.State.Count = 0;
        await _state.WriteStateAsync();
    }
}

[GenerateSerializer]
public class CounterState
{
    [Id(0)]
    public int Count { get; set; } = 0;
}
