using HexMaster.MemeIt.Api.Endpoints;
using HexMaster.MemeIt.Api.Services;
using HexMaster.MemeIt.Games.ExtensionMethods;
using HexMaster.MemeIt.Memes.ExtensionMethods;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using HexMaster.MemeIt.Core.ExtensionMethods;
using HexMaster.MemeIt.Aspire;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization to use camel case by default
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // Use camelCase for API responses
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.AddServiceDefaults();

// Add DAPR client
builder.Services.AddSingleton<DaprClient>(provider => 
{
    return new DaprClientBuilder().Build();
});

builder.AddGamesServices();
builder.AddMemesServices();

// Add WebPubSub service
builder.Services.AddWebPubSubServices();

// Configure JSON serialization globally for the application
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.AddKeyedRedisClient(AspireConstants.RedisCacheName);
builder.Services.AddOpenApi();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register background service for meme template seeding
builder.Services.AddHostedService<MemeTemplateSeederBackgroundService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Use CORS middleware
app.UseCors();

// Use DAPR
app.UseCloudEvents();
app.MapSubscribeHandler();

app.MapGamesEndpoints();
app.MapMemeEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar", opts =>
    {
        opts.DarkMode = true;
        opts.DefaultHttpClient =
            new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.Run();

