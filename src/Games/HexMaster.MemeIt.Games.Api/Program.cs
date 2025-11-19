using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Api.Endpoints;
using HexMaster.MemeIt.Games.Application.Games;
using HexMaster.MemeIt.Games.Application.Services;
using HexMaster.MemeIt.Games.Data.MongoDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("games-db");
builder.Services.AddOpenApi();
builder.Services.AddGamesMongoData(builder.Configuration);
builder.Services.AddSingleton<IGameCodeGenerator, RandomGameCodeGenerator>();
builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
builder.Services.AddScoped<ICommandHandler<CreateGameCommand, CreateGameResult>, CreateGameCommandHandler>();
builder.Services.AddScoped<ICommandHandler<JoinGameCommand, JoinGameResult>, JoinGameCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemovePlayerCommand, RemovePlayerResult>, RemovePlayerCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SetPlayerReadyCommand, SetPlayerReadyResult>, SetPlayerReadyCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemovePlayerCommand, RemovePlayerResult>, RemovePlayerCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SetPlayerReadyCommand, SetPlayerReadyResult>, SetPlayerReadyCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Title = "Meme-It Games API");
}

app.MapDefaultEndpoints();
app.MapGamesEndpoints();

app.MapGet("/", () => Results.Ok("Meme-It Games API"))
   .WithTags("Diagnostics")
   .WithName("GetRoot");

app.Run();
