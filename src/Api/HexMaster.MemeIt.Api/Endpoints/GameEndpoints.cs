using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Games.DataTransferObjects;
using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.GetGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
using HexMaster.MemeIt.Games.Features.LeaveGame;
using HexMaster.MemeIt.Games.Features.StartGame;
using HexMaster.MemeIt.Games.Features.UpdateSettings;
using Localizr.Core.Abstractions.Cqrs;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.MemeIt.Api.Endpoints;

public static class GameEndpoints
{
    public static void MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Games");

        //group.MapGet("/", ProjectList)
        //    .WithName(nameof(ProjectList))
        //    .Produces(StatusCodes.Status200OK);

        group.MapPost("/", CreateGame)
            .WithName(nameof(CreateGame))
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{GameId}", GetGame)
            .WithName(nameof(GetGame))
            .Produces(StatusCodes.Status200OK);


        group.MapPost("/join", JoinGame)
            .WithName(nameof(JoinGame))
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/leave", LeaveGame)
            .WithName(nameof(LeaveGame))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapPost("/start", StartGame)
            .WithName(nameof(StartGame))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapPost("/settings", UpdateSettings)
            .WithName(nameof(UpdateSettings))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }
    private static async Task<IResult> LeaveGame(
        [FromBody] LeaveGameRequest requestPayload,
        [FromServices] ICommandHandler<LeaveGameCommand, object> handler)
    {
        var playerId = requestPayload?.PlayerId;
        var gameCode = requestPayload?.GameCode;
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "PlayerId", ["PlayerId is required."] },
                { "GameCode", ["GameCode is required."] }
            });
        }
        var command = new LeaveGameCommand(playerId, gameCode);
       var responseObject= await handler.HandleAsync(command, CancellationToken.None);
        return Results.Ok(responseObject);
    }

    private static async Task<IResult> StartGame(
        [FromBody] StartGameRequest requestPayload,
        [FromServices] ICommandHandler<StartGameCommand, object> handler)
    {
        var playerId = requestPayload?.PlayerId;
        var gameCode = requestPayload?.GameCode;
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "PlayerId", ["PlayerId is required."] },
                { "GameCode", ["GameCode is required."] }
            });
        }
        var command = new StartGameCommand(playerId, gameCode);
        await handler.HandleAsync(command, CancellationToken.None);
        return Results.Ok();
    }

    private static async Task<IResult> UpdateSettings(
        [FromBody] UpdateSettingsRequest requestPayload,
        [FromServices] ICommandHandler<UpdateSettingsCommand, object> handler)
    {
        var playerId = requestPayload?.PlayerId;
        var gameCode = requestPayload?.GameCode;
        var settings = requestPayload?.Settings ?? new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "PlayerId", ["PlayerId is required."] },
                { "GameCode", ["GameCode is required."] }
            });
        }
        var command = new UpdateSettingsCommand(playerId, settings, gameCode);
        await handler.HandleAsync(command, CancellationToken.None);
        return Results.Ok();
    }

    private static async Task<IResult> CreateGame(
        [FromBody] CreateGameRequest requestPayload, 
        [FromServices] ICommandHandler<CreateGameCommand, CreateGameResponse> handler)
    {
        if (string.IsNullOrWhiteSpace(requestPayload.PlayerName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(requestPayload.PlayerName), ["Player name is required."] }
            });
        }

        var command = new CreateGameCommand
        {
            GameCode = Randomizer.GenerateGameCode(),
            PlayerName = requestPayload.PlayerName,
            Password = requestPayload.Password
        };
        var response = await handler.HandleAsync(command, CancellationToken.None);
        return TypedResults.Created($"/games/{response.GameCode}", response);
    }

    private static async Task<IResult> GetGame(
        [AsParameters] GetGameQuery query,
        [FromServices] IQueryHandler<GetGameQuery, OperationResult< GameDetailsResponse>> handler)
    {
        var response = await handler.HandleAsync(query, CancellationToken.None);
        if (response.Success)
        {
            return Results.Ok(response.ResponseObject);
        }
        return Results.NotFound();
    }
    private static async Task<IResult> JoinGame(
        [FromBody] JoinGameRequest requestPayload,
        [FromServices] ICommandHandler<JoinGameCommand, GameDetailsResponse> handler)
    {

        if (string.IsNullOrWhiteSpace(requestPayload.PlayerName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(requestPayload.PlayerName), ["Player name is required."] }
            });
        }

        var command = new JoinGameCommand
        {
            PlayerName = requestPayload.PlayerName,
            GameCode = requestPayload.GameCode,
            Password = requestPayload.Password
        };
        var response = await handler.HandleAsync(command, CancellationToken.None);
        return Results.Ok(response);
    }

}