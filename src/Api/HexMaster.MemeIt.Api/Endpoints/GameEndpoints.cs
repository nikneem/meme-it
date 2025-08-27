using HexMaster.MemeIt.Core;
using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Core.Services;
using HexMaster.MemeIt.Games.DataTransferObjects;
using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.GetGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
using HexMaster.MemeIt.Games.Features.KickPlayer;
using HexMaster.MemeIt.Games.Features.LeaveGame;
using HexMaster.MemeIt.Games.Features.SetPlayerReadyStatus;
using HexMaster.MemeIt.Games.Features.StartGame;
using HexMaster.MemeIt.Games.Features.UpdateSettings;
using HexMaster.MemeIt.Games.ValueObjects;
using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.Models;
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

        group.MapPost("/kick", KickPlayer)
            .WithName(nameof(KickPlayer))
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

        group.MapPost("/ready", SetPlayerReadyStatus)
            .WithName(nameof(SetPlayerReadyStatus))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapPost("/connection", GetWebPubSubConnection)
            .WithName(nameof(GetWebPubSubConnection))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapPost("/random-memes", GetRandomMemesForGame)
            .WithName(nameof(GetRandomMemesForGame))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }
    private static async Task<IResult> LeaveGame(
        [FromBody] LeaveGameRequest requestPayload,
        [FromServices] ICommandHandler<LeaveGameCommand, object> handler,
        [FromServices] IWebPubSubService webPubSubService)
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
        var responseObject = await handler.HandleAsync(command, CancellationToken.None);

        // Broadcast player left event
        var playerLeftMessage = new GameUpdateMessage
        {
            Type = GameUpdateMessageTypes.PlayerLeft,
            Data = new { playerId },
            GameCode = gameCode
        };
        
        _ = Task.Run(async () => await webPubSubService.BroadcastToGameAsync(gameCode, playerLeftMessage));
        
        return Results.Ok(responseObject);
    }

    private static async Task<IResult> KickPlayer(
        [FromBody] KickPlayerRequest requestPayload,
        [FromServices] ICommandHandler<KickPlayerCommand, GameDetailsResponse> handler)
    {
        var hostPlayerId = requestPayload?.HostPlayerId;
        var targetPlayerId = requestPayload?.TargetPlayerId;
        var gameCode = requestPayload?.GameCode;
        
        if (string.IsNullOrWhiteSpace(hostPlayerId) || string.IsNullOrWhiteSpace(targetPlayerId) || string.IsNullOrWhiteSpace(gameCode))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "HostPlayerId", ["HostPlayerId is required."] },
                { "TargetPlayerId", ["TargetPlayerId is required."] },
                { "GameCode", ["GameCode is required."] }
            });
        }
        
        var command = new KickPlayerCommand(hostPlayerId, targetPlayerId, gameCode);
        var response = await handler.HandleAsync(command, CancellationToken.None);
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateSettings(
        [FromBody] UpdateSettingsRequest requestPayload,
        [FromServices] ICommandHandler<UpdateSettingsCommand, object> handler)
    {
        var playerId = requestPayload?.PlayerId;
        var gameCode = requestPayload?.GameCode;
        var settings = requestPayload?.Settings ?? new GameSettings();
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
        [FromServices] ICommandHandler<JoinGameCommand, GameDetailsResponse> handler,
        [FromServices] IWebPubSubService webPubSubService)
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

        // Broadcast game updated event (contains all player information)
        if (response != null)
        {
            var joinedPlayer = response.Players?.FirstOrDefault(p => p.Name == requestPayload.PlayerName);
            if (joinedPlayer != null)
            {
                Console.WriteLine($"Player {joinedPlayer.Name} (ID: {joinedPlayer.Id}) successfully joined game {requestPayload.GameCode}");
            }
            else
            {
                Console.WriteLine($"Warning: Could not find joined player {requestPayload.PlayerName} in response players list");
            }

            // Broadcast full game update (this contains the new player information)
            var broadcastData = new GameStateBroadcastResponse(
                response.GameCode,
                response.Status,
                response.Players ?? new List<PlayerResponse>(),
                response.IsPasswordProtected,
                response.Settings);
            
            var gameUpdatedMessage = new GameUpdateMessage
            {
                Type = GameUpdateMessageTypes.GameUpdated,
                Data = broadcastData,
                GameCode = requestPayload.GameCode
            };
            
            Console.WriteLine($"Broadcasting GameUpdated message for game {requestPayload.GameCode} with {response.Players?.Count() ?? 0} players");
            await webPubSubService.BroadcastToGameAsync(requestPayload.GameCode, gameUpdatedMessage);
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> SetPlayerReadyStatus(
        [FromBody] SetPlayerReadyStatusRequest requestPayload,
        [FromServices] ICommandHandler<SetPlayerReadyStatusCommand, GameDetailsResponse> handler,
        [FromServices] HexMaster.MemeIt.Core.Services.IWebPubSubService webPubSubService)
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
        var command = new SetPlayerReadyStatusCommand(playerId, gameCode, requestPayload?.IsReady ?? false);
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // Broadcast player ready status changed event
        var playerReadyStatusChangedMessage = new GameUpdateMessage
        {
            Type = GameUpdateMessageTypes.PlayerReadyStatusChanged,
            Data = new { playerId, isReady = requestPayload?.IsReady ?? false },
            GameCode = gameCode
        };
        
        _ = Task.Run(async () => await webPubSubService.BroadcastToGameAsync(gameCode, playerReadyStatusChangedMessage));

        return Results.Ok(response);
    }

    private static async Task<IResult> StartGame(
        [FromBody] StartGameRequest requestPayload,
        [FromServices] ICommandHandler<StartGameCommand, GameDetailsResponse> handler,
        [FromServices] IWebPubSubService webPubSubService)
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
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // Broadcast game started event
        var gameStartedMessage = new GameUpdateMessage
        {
            Type = GameUpdateMessageTypes.GameStarted,
            Data = new
            {
                gameCode,
                status = "InProgress",
                startedAt = DateTime.UtcNow,
                startedBy = playerId
            },
            GameCode = gameCode
        };

        _ = Task.Run(async () => await webPubSubService.BroadcastToGameAsync(gameCode, gameStartedMessage));

        return Results.Ok(response);
    }

    private static async Task<IResult> GetWebPubSubConnection(
        [FromBody] WebPubSubConnectionRequest requestPayload,
        [FromServices] IQueryHandler<GetGameQuery, OperationResult<GameDetailsResponse>> gameHandler,
        [FromServices] HexMaster.MemeIt.Core.Services.IWebPubSubService webPubSubService)
    {
        var gameCode = requestPayload?.GameCode;
        var playerId = requestPayload?.PlayerId;

        if (string.IsNullOrWhiteSpace(gameCode) || string.IsNullOrWhiteSpace(playerId))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "GameCode", ["GameCode is required."] },
                { "PlayerId", ["PlayerId is required."] }
            });
        }

        try
        {
            // First validate that the player exists in the game
            var query = new GetGameQuery(gameCode, playerId);
            var gameResult = await gameHandler.HandleAsync(query, CancellationToken.None);

            if (!gameResult.Success || gameResult.ResponseObject?.Players?.Any(p => p.Id == playerId) != true)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Player", ["Player is not part of this game."] }
                });
            }

            // Generate Web PubSub connection URL
            var connectionResponse = await webPubSubService.GenerateConnectionUrlAsync(gameCode, playerId);

            if (!connectionResponse.IsSuccess)
            {
                return Results.Problem(connectionResponse.ErrorMessage ?? "Failed to generate Web PubSub connection", statusCode: 500);
            }

            return Results.Ok(connectionResponse);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to generate Web PubSub connection", statusCode: 500);
        }
    }

    private static async Task<IResult> GetRandomMemesForGame(
        [FromBody] GetRandomMemesRequest requestPayload,
        [FromServices] IQueryHandler<GetGameQuery, OperationResult<GameDetailsResponse>> gameHandler,
        [FromServices] HexMaster.MemeIt.Memes.Abstractions.IMemeTemplateRepository memeRepository)
    {
        var gameCode = requestPayload?.GameCode;
        var playerId = requestPayload?.PlayerId;

        if (string.IsNullOrWhiteSpace(gameCode) || string.IsNullOrWhiteSpace(playerId))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "GameCode", ["GameCode is required."] },
                { "PlayerId", ["PlayerId is required."] }
            });
        }

        try
        {
            // First validate that the player exists in the game and game is active
            var query = new GetGameQuery(gameCode, playerId);
            var gameResult = await gameHandler.HandleAsync(query, CancellationToken.None);

            if (!gameResult.Success)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Game", ["Game not found or player is not part of this game."] }
                });
            }

            var game = gameResult.ResponseObject;
            if (game?.Players?.Any(p => p.Id == playerId) != true)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Player", ["Player is not part of this game."] }
                });
            }

            // Only allow this when game is active/in progress
            if (game.Status != "Active" && game.Status != "InProgress")
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Game", ["Game must be active to get random memes."] }
                });
            }

            // Get all available memes
            var allMemes = await memeRepository.GetAllAsync();
            var memeList = allMemes.ToList();

            if (!memeList.Any())
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Memes", ["No memes available."] }
                });
            }

            // Select a random meme for this player
            var random = new Random();
            var randomIndex = random.Next(0, memeList.Count);
            var selectedMeme = memeList[randomIndex];

            var response = new GetRandomMemeResponse
            {
                PlayerId = playerId,
                GameCode = gameCode,
                MemeTemplate = new RandomMemeTemplateResponse
                {
                    Id = selectedMeme.Id,
                    Name = selectedMeme.Name,
                    Description = selectedMeme.Description ?? string.Empty,
                    ImageUrl = selectedMeme.SourceImageUrl,
                    SourceWidth = selectedMeme.SourceWidth,
                    SourceHeight = selectedMeme.SourceHeight,
                    TextAreas = selectedMeme.TextAreas.Select((ta, index) => new RandomMemeTextAreaResponse
                    {
                        Id = index.ToString(), // Using index as ID since TextArea doesn't have ID
                        X = ta.X,
                        Y = ta.Y,
                        Width = ta.Width,
                        Height = ta.Height,
                        MaxLength = ta.MaxLength,
                        FontSize = ta.FontSize,
                        FontFamily = ta.FontFamily,
                        Color = ta.FontColor,
                        IsBold = ta.FontBold,
                        IsItalic = false, // Not available in current model
                        IsUnderline = false, // Not available in current model
                        TextAlign = "left", // Default value
                        VerticalAlign = "middle" // Default value
                    }).ToList()
                },
                AssignedAt = DateTime.UtcNow
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get random meme: {ex.Message}", statusCode: 500);
        }
    }
}