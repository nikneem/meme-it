using Microsoft.AspNetCore.SignalR;

namespace HexMaster.MemeIt.Realtime.Api.Hubs;

/// <summary>
/// SignalR hub for broadcasting real-time game updates to connected clients.
/// Clients connect to this hub to receive notifications about game state changes,
/// player actions, and other game-related events.
/// </summary>
public sealed class GamesHub : Hub
{
    /// <summary>
    /// Called when a client connects to the hub.
    /// Can be used for logging or connection management if needed.
    /// </summary>
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// Can be used for cleanup or logging if needed.
    /// </summary>
    /// <param name="exception">Exception that caused the disconnect, if any.</param>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows clients to join a game-specific group to receive updates for that game only.
    /// </summary>
    /// <param name="gameCode">The game code to join.</param>
    public async Task JoinGameGroup(string gameCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{gameCode}");
    }

    /// <summary>
    /// Allows clients to leave a game-specific group.
    /// </summary>
    /// <param name="gameCode">The game code to leave.</param>
    public async Task LeaveGameGroup(string gameCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{gameCode}");
    }
}
