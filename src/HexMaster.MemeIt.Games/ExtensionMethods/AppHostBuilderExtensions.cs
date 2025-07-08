using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features.GetGame;
using HexMaster.MemeIt.Games.Features.JoinGame;
using Localizr.Core.Abstractions.Cqrs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HexMaster.MemeIt.Games.ExtensionMethods;

public static class AppHostBuilderExtensions
{
    public static IHostApplicationBuilder AddGamesServices(this IHostApplicationBuilder builder, bool addLocalizrCosmosDb = false)
    {

        builder.Services.AddTransient<ICommandHandler<CreateGameCommand, CreateGameResponse>, CreateGameCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<JoinGameCommand, GameDetailsResponse>, JoinGameCommandHandler>();

        builder.Services.AddTransient<IQueryHandler<GetGameQuery, OperationResult< GameDetailsResponse>>, GetGameQueryHandler>();

        return builder;
    }
}