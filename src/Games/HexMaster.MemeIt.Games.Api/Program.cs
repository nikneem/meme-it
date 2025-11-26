using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Application.Games;
using HexMaster.MemeIt.Games.Abstractions.Application.Queries;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Api.Endpoints;
using HexMaster.MemeIt.Games.Api.Infrastructure;
using HexMaster.MemeIt.Games.Api.Infrastructure.Identity;
using HexMaster.MemeIt.Games.Application.Games;
using HexMaster.MemeIt.Games.Application.Services;
using HexMaster.MemeIt.Games.Data.MongoDb;
using HexMaster.MemeIt.IntegrationEvents.Publishers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("games-db");
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Authorization")
              .AllowCredentials();
    });
});
builder.Services.AddGamesMongoData(builder.Configuration);
builder.Services.AddScheduledTaskService();
builder.Services.AddSingleton<IGameCodeGenerator, RandomGameCodeGenerator>();
builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
builder.Services.Configure<UsersJwtOptions>(builder.Configuration.GetSection(UsersJwtOptions.SectionName));
builder.Services.AddSingleton<IPlayerIdentityProvider, JwtPlayerIdentityProvider>();
builder.Services.AddScoped<ICommandHandler<CreateGameCommand, CreateGameResult>, CreateGameCommandHandler>();
builder.Services.AddScoped<ICommandHandler<JoinGameCommand, JoinGameResult>, JoinGameCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemovePlayerCommand, RemovePlayerResult>, RemovePlayerCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SetPlayerReadyCommand, SetPlayerReadyResult>, SetPlayerReadyCommandHandler>();
builder.Services.AddScoped<ICommandHandler<StartGameCommand, StartGameResult>, StartGameCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SelectMemeTemplateCommand, SelectMemeTemplateResult>, SelectMemeTemplateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SubmitMemeCommand, SubmitMemeResult>, SubmitMemeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>, EndCreativePhaseCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RateMemeCommand, RateMemeResult>, RateMemeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult>, EndScorePhaseCommandHandler>();
builder.Services.AddScoped<ICommandHandler<EndRoundCommand, EndRoundResult>, EndRoundCommandHandler>();
builder.Services.AddScoped<ICommandHandler<StartNewRoundCommand, StartNewRoundResult>, StartNewRoundCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetGameDetailsQuery, GetGameDetailsResult>, GetGameDetailsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetPlayerRoundStateQuery, GetPlayerRoundStateResult>, GetPlayerRoundStateQueryHandler>();

// Add Dapr client and register IntegrationEvents publisher implementation
builder.Services.AddDaprClient();
builder.Services.AddScoped<IIntegrationEventPublisher, DaprIntegrationEventPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Title = "Meme-It Games API");
}

app.UseCloudEvents();
app.UseCors("AllowAngularApp");
app.MapDefaultEndpoints();
app.MapGamesEndpoints();
app.MapSchedulingEndpoints();

app.MapGet("/", () => Results.Ok("Meme-It Games API"))
   .WithTags("Diagnostics")
   .WithName("GetRoot");

app.Run();
