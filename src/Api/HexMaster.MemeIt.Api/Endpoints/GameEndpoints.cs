using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Games.DataTransferObjects;
using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.GetGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
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