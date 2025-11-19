using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Games.Abstractions;
using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.GetGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
using HexMaster.MemeIt.Games.Features.KickPlayer;
using HexMaster.MemeIt.Games.Features.LeaveGame;
using HexMaster.MemeIt.Games.Features.SetPlayerReadyStatus;
using HexMaster.MemeIt.Games.Features.StartGame;
using HexMaster.MemeIt.Games.Features.UpdateSettings;
using HexMaster.MemeIt.Games.Repositories;
using HexMaster.MemeIt.Games.Services;
using Localizr.Core.Abstractions.Cqrs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HexMaster.MemeIt.Games.ExtensionMethods;

public static class AppHostBuilderExtensions
{
    public static IHostApplicationBuilder AddGamesServices(this IHostApplicationBuilder builder, bool addLocalizrCosmosDb = false)
    {
        // Register game services
        builder.Services.AddScoped<IGameRepository, DaprGameRepository>();
        builder.Services.AddScoped<IGameService, GameService>();

        // Register command and query handlers
        builder.Services.AddTransient<ICommandHandler<CreateGameCommand, CreateGameResponse>, CreateGameCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<JoinGameCommand, GameDetailsResponse>, JoinGameCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<LeaveGameCommand, LeaveGameResponse>, LeaveGameCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<KickPlayerCommand, GameDetailsResponse>, KickPlayerCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<SetPlayerReadyStatusCommand, GameDetailsResponse>, SetPlayerReadyStatusCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<StartGameCommand, GameDetailsResponse>, StartGameCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<UpdateSettingsCommand, GameDetailsResponse>, UpdateSettingsCommandHandler>();

        builder.Services.AddTransient<IQueryHandler<GetGameQuery, OperationResult< GameDetailsResponse>>, GetGameQueryHandler>();

        return builder;
    }
}