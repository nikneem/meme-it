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

        group.MapPost("/join", JoinGameAsync)
            .Accepts<JoinGameRequest>(MediaTypeNames.Application.Json)
            .WithName("JoinGame")
            .WithSummary("Allows a player to join an existing game by game code.")
            .Produces<JoinGameResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
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

        if (string.IsNullOrWhiteSpace(request.GameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.GameCode)] = new[] { "Game code is required." }
            });
        }

        var command = new JoinGameCommand(playerId, request.PlayerName, request.GameCode, request.Password);
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
