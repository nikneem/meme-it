using Dapr;
using HexMaster.MemeIt.IntegrationEvents.Events;
using HexMaster.MemeIt.Realtime.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HexMaster.MemeIt.Realtime.Api.Endpoints;

/// <summary>
/// Endpoints for Dapr pubsub subscriptions.
/// These endpoints receive integration events from the Dapr pubsub component
/// and broadcast them to SignalR clients.
/// </summary>
public static class DaprSubscriptionsEndpoints
{
    private const string PubSubName = "chatservice-pubsub";

    public static IEndpointRouteBuilder MapDaprSubscriptionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/dapr/subscriptions")
            .WithTags("Dapr Subscriptions");

        // Subscribe to player state changed events
        group.MapPost("/playerstatechanged", OnPlayerStateChangedAsync)
            .WithTopic(PubSubName, "playerstatechanged")
            .WithName("PlayerStateChangedSubscription")
            .WithSummary("Handles player state changed events from Dapr pubsub")
            .ExcludeFromDescription(); // Hide from OpenAPI as this is for Dapr only

        // Subscribe to player removed events
        group.MapPost("/playerremoved", OnPlayerRemovedAsync)
            .WithTopic(PubSubName, "playerremoved")
            .WithName("PlayerRemovedSubscription")
            .WithSummary("Handles player removed events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to player joined events
        group.MapPost("/playerjoined", OnPlayerJoinedAsync)
            .WithTopic(PubSubName, "playerjoined")
            .WithName("PlayerJoinedSubscription")
            .WithSummary("Handles player joined events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to game started events
        group.MapPost("/gamestarted", OnGameStartedAsync)
            .WithTopic(PubSubName, "gamestarted")
            .WithName("GameStartedSubscription")
            .WithSummary("Handles game started events from Dapr pubsub")
            .ExcludeFromDescription();

        return endpoints;
    }

    /// <summary>
    /// Handles PlayerStateChangedEvent from Dapr pubsub and broadcasts to SignalR clients.
    /// </summary>
    private static async Task<IResult> OnPlayerStateChangedAsync(
        [FromBody] PlayerStateChangedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received PlayerStateChanged event: PlayerId={PlayerId}, DisplayName={DisplayName}, IsReady={IsReady}, GameCode={GameCode}",
                @event.PlayerId, @event.DisplayName, @event.IsReady, @event.GameCode);

            // Broadcast to all connected clients
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "PlayerStateChanged",
                new
                {
                    @event.PlayerId,
                    @event.DisplayName,
                    @event.IsReady,
                    @event.GameCode
                },
                cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing PlayerStateChanged event");
            // Return 200 OK to prevent Dapr from retrying
            // (already logged for monitoring)
            return Results.Ok();
        }
    }

    /// <summary>
    /// Handles PlayerRemovedEvent from Dapr pubsub and broadcasts to SignalR clients.
    /// </summary>
    private static async Task<IResult> OnPlayerRemovedAsync(
        [FromBody] PlayerRemovedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received PlayerRemoved event: PlayerId={PlayerId}, DisplayName={DisplayName}, GameCode={GameCode}",
                @event.PlayerId, @event.DisplayName, @event.GameCode);

            // Broadcast to all connected clients
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "PlayerRemoved",
                new
                {
                    @event.PlayerId,
                    @event.DisplayName,
                    @event.GameCode
                },
                cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing PlayerRemoved event");
            // Return 200 OK to prevent Dapr from retrying
            return Results.Ok();
        }
    }

    /// <summary>
    /// Handles PlayerJoinedEvent from Dapr pubsub and broadcasts to SignalR clients.
    /// </summary>
    private static async Task<IResult> OnPlayerJoinedAsync(
        [FromBody] PlayerJoinedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received PlayerJoined event: PlayerId={PlayerId}, DisplayName={DisplayName}, GameCode={GameCode}",
                @event.PlayerId, @event.DisplayName, @event.GameCode);

            // Broadcast to all connected clients
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "PlayerJoined",
                new
                {
                    @event.PlayerId,
                    @event.DisplayName,
                    @event.GameCode
                },
                cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing PlayerJoined event");
            // Return 200 OK to prevent Dapr from retrying
            // (already logged for monitoring)
            return Results.Ok();
        }
    }

    /// <summary>
    /// Handles GameStartedEvent from Dapr pubsub and broadcasts to SignalR clients.
    /// </summary>
    private static async Task<IResult> OnGameStartedAsync(
        [FromBody] GameStartedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received GameStarted event: GameCode={GameCode}, RoundNumber={RoundNumber}",
                @event.GameCode, @event.RoundNumber);

            // Broadcast to all connected clients in the game group
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "GameStarted",
                new
                {
                    @event.GameCode,
                    @event.RoundNumber
                },
                cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing GameStarted event");
            // Return 200 OK to prevent Dapr from retrying
            return Results.Ok();
        }
    }
}
