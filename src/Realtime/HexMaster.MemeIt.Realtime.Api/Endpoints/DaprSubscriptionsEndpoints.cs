using Dapr;
using HexMaster.MemeIt.IntegrationEvents;
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

    public static IEndpointRouteBuilder MapDaprSubscriptionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/dapr/subscriptions")
            .WithTags("Dapr Subscriptions");

        // Subscribe to player state changed events
        group.MapPost("/playerstatechanged", OnPlayerStateChangedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.PlayerStateChanged)
            .WithName("PlayerStateChangedSubscription")
            .WithSummary("Handles player state changed events from Dapr pubsub")
            .ExcludeFromDescription(); // Hide from OpenAPI as this is for Dapr only

        // Subscribe to player removed events
        group.MapPost("/playerremoved", OnPlayerRemovedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.PlayerRemoved)
            .WithName("PlayerRemovedSubscription")
            .WithSummary("Handles player removed events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to player joined events
        group.MapPost("/playerjoined", OnPlayerJoinedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.PlayerJoined)
            .WithName("PlayerJoinedSubscription")
            .WithSummary("Handles player joined events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to game started events
        group.MapPost("/gamestarted", OnGameStartedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.GameStarted)
            .WithName("GameStartedSubscription")
            .WithSummary("Handles game started events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to round started events
        group.MapPost("/roundstarted", OnRoundStartedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.RoundStarted)
            .WithName("RoundStartedSubscription")
            .WithSummary("Handles round started events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to creative phase ended events
        group.MapPost("/creativephaseended", OnCreativePhaseEndedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.CreativePhaseEnded)
            .WithName("CreativePhaseEndedSubscription")
            .WithSummary("Handles creative phase ended events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to score phase started events
        group.MapPost("/scorephasestarted", OnScorePhaseStartedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.ScorePhaseStarted)
            .WithName("ScorePhaseStartedSubscription")
            .WithSummary("Handles score phase started events from Dapr pubsub")
            .ExcludeFromDescription();

        // Subscribe to round ended events
        group.MapPost("/roundended", OnRoundEndedAsync)
            .WithTopic(DaprConstants.PubSubName, DaprConstants.Topics.RoundEnded)
            .WithName("RoundEndedSubscription")
            .WithSummary("Handles round ended events from Dapr pubsub")
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

    /// <summary>
    /// Handles RoundStartedEvent from Dapr pubsub and broadcasts to SignalR clients.
    /// </summary>
    private static async Task<IResult> OnRoundStartedAsync(
        [FromBody] RoundStartedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received RoundStarted event: GameCode={GameCode}, RoundNumber={RoundNumber}",
                @event.GameCode, @event.RoundNumber);

            // Broadcast to all connected clients in the game group
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "RoundStarted",
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
            logger.LogError(ex, "Error processing RoundStarted event");
            // Return 200 OK to prevent Dapr from retrying
            return Results.Ok();
        }
    }

    /// <summary>
    /// Handles CreativePhaseEndedEvent from Dapr pubsub and broadcasts to SignalR clients with the first meme to score.
    /// </summary>
    private static async Task<IResult> OnCreativePhaseEndedAsync(
        [FromBody] CreativePhaseEndedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received CreativePhaseEnded event: GameCode={GameCode}, RoundNumber={RoundNumber}",
                @event.GameCode, @event.RoundNumber);

            // Broadcast to all connected clients in the game group
            // The frontend will request the first meme to score from the Games API
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "CreativePhaseEnded",
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
            logger.LogError(ex, "Error processing CreativePhaseEnded event");
            // Return 200 OK to prevent Dapr from retrying
            return Results.Ok();
        }
    }

    /// <summary>
    /// Handles ScorePhaseStartedEvent from Dapr pubsub and broadcasts to SignalR clients with the meme to rate.
    /// </summary>
    private static async Task<IResult> OnScorePhaseStartedAsync(
        [FromBody] ScorePhaseStartedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received ScorePhaseStarted event: GameCode={GameCode}, RoundNumber={RoundNumber}, MemeId={MemeId}, PlayerId={PlayerId}",
                @event.GameCode, @event.RoundNumber, @event.MemeId, @event.PlayerId);

            // Broadcast to all connected clients in the game group
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "ScorePhaseStarted",
                new
                {
                    @event.GameCode,
                    @event.RoundNumber,
                    @event.MemeId,
                    @event.PlayerId,
                    @event.MemeTemplateId,
                    @event.TextEntries,
                    @event.RatingDurationSeconds
                },
                cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ScorePhaseStarted event");
            // Return 200 OK to prevent Dapr from retrying
            return Results.Ok();
        }
    }

    /// <summary>
    /// Handles RoundEndedEvent from Dapr pubsub and broadcasts to SignalR clients with the scoreboard.
    /// </summary>
    private static async Task<IResult> OnRoundEndedAsync(
        [FromBody] RoundEndedEvent @event,
        IHubContext<GamesHub> hubContext,
        ILogger<GamesHub> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Received RoundEnded event: GameCode={GameCode}, RoundNumber={RoundNumber}, TotalRounds={TotalRounds}",
                @event.GameCode, @event.RoundNumber, @event.TotalRounds);

            // Broadcast to all connected clients in the game group
            await hubContext.Clients.Group(@event.GameCode).SendAsync(
                "RoundEnded",
                new
                {
                    @event.GameCode,
                    @event.RoundNumber,
                    @event.TotalRounds,
                    @event.Scoreboard
                },
                cancellationToken);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing RoundEnded event");
            // Return 200 OK to prevent Dapr from retrying
            return Results.Ok();
        }
    }
}
