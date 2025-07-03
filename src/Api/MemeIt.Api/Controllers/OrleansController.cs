using Microsoft.AspNetCore.Mvc;
using Orleans;
using MemeIt.Api.Orleans.Interfaces;

namespace MemeIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrleansController : ControllerBase
{
    private readonly IClusterClient _clusterClient;

    public OrleansController(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    [HttpGet("counter/{id}")]
    public async Task<IActionResult> GetCounter(string id)
    {
        var grain = _clusterClient.GetGrain<IMemeCounterGrain>(id);
        var count = await grain.GetCountAsync();
        return Ok(new { Id = id, Count = count });
    }

    [HttpPost("counter/{id}/increment")]
    public async Task<IActionResult> IncrementCounter(string id)
    {
        var grain = _clusterClient.GetGrain<IMemeCounterGrain>(id);
        var newCount = await grain.IncrementAsync();
        return Ok(new { Id = id, Count = newCount });
    }

    [HttpPost("counter/{id}/reset")]
    public async Task<IActionResult> ResetCounter(string id)
    {
        var grain = _clusterClient.GetGrain<IMemeCounterGrain>(id);
        await grain.ResetAsync();
        return Ok(new { Id = id, Message = "Counter reset" });
    }
}
