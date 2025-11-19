using System.Net.Mime;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Api.Infrastructure;
using HexMaster.MemeIt.Games.Api.Requests;
using HexMaster.MemeIt.Games.Api.Responses;
using HexMaster.MemeIt.Games.Application.Games;

namespace HexMaster.MemeIt.Games.Api.Endpoints;

public static class GamesEndpoints
{
    public static IEndpointRouteBuilder MapGamesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/games")
            .WithTags("Games");

        group.MapPost("/", CreateGameAsync)
            .Accepts<CreateGameRequest>(MediaTypeNames.Application.Json)
            .WithName("CreateGame")
            .WithSummary("Creates a new game and assigns the caller as the admin.")
            .Produces<CreateGameResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{gameCode}/remove-player/{playerId:guid}", RemovePlayerAsync)
            .WithName("RemovePlayer")
            .WithSummary("Removes a player from the game. Only the admin can perform this action.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPatch("/{gameCode}/ready", SetPlayerReadyAsync)
            .WithName("SetPlayerReady")
            .WithSummary("Sets the player's ready state in the lobby.")
            .Produces<SetPlayerReadyResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);


        group.MapPost("/{gameCode}/join", JoinGameAsync)
            .Accepts<JoinGameRequest>(MediaTypeNames.Application.Json)
            .WithName("JoinGame")
            .WithSummary("Allows a player to join an existing game by game code.")
            .Produces<JoinGameResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> RemovePlayerAsync(
        HttpContext httpContext,
        string gameCode,
        Guid playerId,
        ICommandHandler<RemovePlayerCommand, RemovePlayerResult> handler,
        CancellationToken cancellationToken)
    {
        if (!PlayerIdentityHelper.TryParsePlayerId(httpContext.Request.Headers, out var adminPlayerId, out var error))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [PlayerIdentityHelper.PlayerIdHeaderName] = new[] { error ?? "Invalid player id." }
            });
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        if (playerId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(playerId)] = new[] { "Player id is required." }
            });
        }

        var command = new RemovePlayerCommand(adminPlayerId, gameCode, playerId);
        try
        {
            await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Results.NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["game"] = new[] { ex.Message }
            });
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "payload"] = new[] { ex.Message }
            });
        }
    }

    private static async Task<IResult> SetPlayerReadyAsync(
        HttpContext httpContext,
        string gameCode,
        ICommandHandler<SetPlayerReadyCommand, SetPlayerReadyResult> handler,
        CancellationToken cancellationToken,
        bool isReady = true)
    {
        if (!PlayerIdentityHelper.TryParsePlayerId(httpContext.Request.Headers, out var playerId, out var error))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [PlayerIdentityHelper.PlayerIdHeaderName] = new[] { error ?? "Invalid player id." }
            });
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        var command = new SetPlayerReadyCommand(playerId, gameCode, isReady);
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            var response = new SetPlayerReadyResponse(result.PlayerId, result.IsReady, result.AllPlayersReady);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["game"] = new[] { ex.Message }
            });
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "payload"] = new[] { ex.Message }
            });
        }
    }

    private static async Task<IResult> CreateGameAsync(
        HttpContext httpContext,
        CreateGameRequest request,
        ICommandHandler<CreateGameCommand, CreateGameResult> handler,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["body"] = new[] { "Request payload is required." }
            });
        }

        if (!PlayerIdentityHelper.TryParsePlayerId(httpContext.Request.Headers, out var playerId, out var error))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [PlayerIdentityHelper.PlayerIdHeaderName] = new[] { error ?? "Invalid player id." }
            });
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.DisplayName)] = new[] { "Display name is required." }
            });
        }

        var command = new CreateGameCommand(playerId, request.DisplayName, request.Password);
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            var response = new CreateGameResponse(result.GameCode, result.AdminPlayerId, result.CreatedAt, result.State.Name);
            return Results.Created($"/api/games/{response.GameCode}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "payload"] = new[] { ex.Message }
            });
        }
    }

    private static async Task<IResult> JoinGameAsync(
        HttpContext httpContext,
        string gameCode,
        JoinGameRequest request,
        ICommandHandler<JoinGameCommand, JoinGameResult> handler,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["body"] = new[] { "Request payload is required." }
            });
        }

        if (!PlayerIdentityHelper.TryParsePlayerId(httpContext.Request.Headers, out var playerId, out var error))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [PlayerIdentityHelper.PlayerIdHeaderName] = new[] { error ?? "Invalid player id." }
            });
        }

        if (string.IsNullOrWhiteSpace(request.PlayerName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.PlayerName)] = new[] { "Player name is required." }
            });
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        var command = new JoinGameCommand(playerId, request.PlayerName, gameCode, request.Password);
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            var response = new JoinGameResponse(result.GameCode, result.PlayerId, result.State.Name);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["game"] = new[] { ex.Message }
            });
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "payload"] = new[] { ex.Message }
            });
        }
    }
}
