using System;
using System.Linq;
using System.Net.Mime;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Application.Games;
using HexMaster.MemeIt.Games.Abstractions.Application.Queries;
using HexMaster.MemeIt.Games.Api.Infrastructure.Identity;
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

        group.MapGet("/{gameCode}", GetGameDetailsAsync)
            .WithName("GetGameDetails")
            .WithSummary("Retrieves game details by game code. Requires player to be part of the game.")
            .Produces<GetGameDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
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

        group.MapPost("/{gameCode}/start", StartGameAsync)
            .WithName("StartGame")
            .WithSummary("Starts the game and begins the first round. Only the admin can start the game.")
            .Produces<StartGameResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{gameCode}/rounds/{roundNumber:int}/select-meme", SelectMemeTemplateAsync)
            .WithName("SelectMemeTemplate")
            .WithSummary("Selects a meme template for the current round.")
            .Produces<SelectMemeTemplateResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{gameCode}/select-meme", GetPlayerRoundStateAsync)
            .WithName("GetPlayerRoundState")
            .WithSummary("Gets the current player's round state including selected meme.")
            .Produces<GetPlayerRoundStateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{gameCode}/rounds/{roundNumber:int}/rate", RateMemeAsync)
            .Accepts<RateMemeRequest>(MediaTypeNames.Application.Json)
            .WithName("RateMeme")
            .WithSummary("Submits a rating for a meme in the current round.")
            .Produces<RateMemeResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static IResult? TryResolveIdentity(HttpRequest request, IPlayerIdentityProvider provider, out PlayerIdentity identity)
    {
        try
        {
            identity = provider.GetIdentity(request);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            identity = default!;
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: ex.Message);
        }
    }

    private static async Task<IResult> RemovePlayerAsync(
        HttpContext httpContext,
        string gameCode,
        Guid playerId,
        ICommandHandler<RemovePlayerCommand, RemovePlayerResult> handler,
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        if (TryResolveIdentity(httpContext.Request, identityProvider, out var adminIdentity) is { } identityError)
        {
            return identityError;
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

        var command = new RemovePlayerCommand(adminIdentity.UserId, gameCode, playerId);
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
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken,
        bool isReady = true)
    {
        if (TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity) is { } identityError)
        {
            return identityError;
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        var command = new SetPlayerReadyCommand(playerIdentity.UserId, gameCode, isReady);
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
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["body"] = new[] { "Request payload is required." }
            });
        }

        if (TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity) is { } identityError)
        {
            return identityError;
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName) &&
            !string.Equals(request.DisplayName.Trim(), playerIdentity.DisplayName, StringComparison.Ordinal))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.DisplayName)] = new[] { "Display name must match the authenticated player." }
            });
        }

        var command = new CreateGameCommand(playerIdentity.UserId, playerIdentity.DisplayName, request.Password);
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
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["body"] = new[] { "Request payload is required." }
            });
        }

        if (TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity) is { } identityError)
        {
            return identityError;
        }

        if (!string.IsNullOrWhiteSpace(request.PlayerName) &&
            !string.Equals(request.PlayerName.Trim(), playerIdentity.DisplayName, StringComparison.Ordinal))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.PlayerName)] = new[] { "Player name must match the authenticated player." }
            });
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        var command = new JoinGameCommand(playerIdentity.UserId, playerIdentity.DisplayName, gameCode, request.Password);
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

    private static async Task<IResult> GetGameDetailsAsync(
        HttpContext httpContext,
        string gameCode,
        IQueryHandler<GetGameDetailsQuery, GetGameDetailsResult> handler,
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        var errorResult = TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity);
        if (errorResult is not null)
        {
            return errorResult;
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        var query = new GetGameDetailsQuery(gameCode, playerIdentity.UserId);

        try
        {
            var result = await handler.HandleAsync(query, cancellationToken).ConfigureAwait(false);

            var response = new GetGameDetailsResponse(
                result.GameCode,
                result.State,
                result.CreatedAt,
                result.Players.Select(p => new PlayerDetailsDto(p.PlayerId, p.DisplayName, p.IsReady)).ToArray(),
                result.Rounds.Select(r => new RoundDetailsDto(r.RoundNumber, r.SubmissionCount)).ToArray(),
                result.IsAdmin);

            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "payload"] = new[] { ex.Message }
            });
        }
    }

    private static async Task<IResult> StartGameAsync(
        HttpContext httpContext,
        string gameCode,
        ICommandHandler<StartGameCommand, StartGameResult> handler,
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        if (TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity) is { } identityError)
        {
            return identityError;
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        var command = new StartGameCommand(gameCode, playerIdentity.UserId);
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            var response = new StartGameResponse(result.GameCode, result.RoundNumber);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
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

    private static async Task<IResult> SelectMemeTemplateAsync(
        HttpContext httpContext,
        string gameCode,
        int roundNumber,
        SelectMemeTemplateRequest request,
        ICommandHandler<SelectMemeTemplateCommand, SelectMemeTemplateResult> handler,
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        if (TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity) is { } identityError)
        {
            return identityError;
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        if (request?.MemeTemplateId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.MemeTemplateId)] = new[] { "Meme template ID is required." }
            });
        }

        var command = new SelectMemeTemplateCommand(gameCode, playerIdentity.UserId, roundNumber, request!.MemeTemplateId);
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            var response = new SelectMemeTemplateResponse(result.GameCode, result.PlayerId, result.RoundNumber, result.MemeTemplateId);
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

    private static async Task<IResult> GetPlayerRoundStateAsync(
        HttpContext httpContext,
        string gameCode,
        IQueryHandler<GetPlayerRoundStateQuery, GetPlayerRoundStateResult> handler,
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        if (TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity) is { } identityError)
        {
            return identityError;
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        var query = new GetPlayerRoundStateQuery(gameCode, playerIdentity.UserId);
        try
        {
            var result = await handler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
            var response = new GetPlayerRoundStateResponse(
                result.GameCode,
                result.PlayerId,
                result.RoundNumber,
                result.RoundStartedAt,
                result.SelectedMemeTemplateId);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
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

    private static async Task<IResult> RateMemeAsync(
        HttpContext httpContext,
        string gameCode,
        int roundNumber,
        RateMemeRequest request,
        ICommandHandler<RateMemeCommand, RateMemeResult> handler,
        IPlayerIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        if (TryResolveIdentity(httpContext.Request, identityProvider, out var playerIdentity) is { } identityError)
        {
            return identityError;
        }

        if (string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(gameCode)] = new[] { "Game code is required." }
            });
        }

        if (roundNumber <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(roundNumber)] = new[] { "Round number must be greater than 0." }
            });
        }

        if (request.Rating < 0 || request.Rating > 5)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Rating)] = new[] { "Rating must be between 0 and 5." }
            });
        }

        var command = new RateMemeCommand(gameCode, roundNumber, request.MemeId, playerIdentity.UserId, request.Rating);
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            var response = new RateMemeResponse(result.Success);
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
                ["rating"] = new[] { ex.Message }
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
