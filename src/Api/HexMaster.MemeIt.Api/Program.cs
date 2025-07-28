using HexMaster.MemeIt.Api.Endpoints;
using HexMaster.MemeIt.Games.ExtensionMethods;
using HexMaster.MemeIt.Memes.ExtensionMethods;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddGamesServices();
builder.AddMemesServices();

builder.AddKeyedRedisClient(AspireConstants.RedisCacheName);
builder.UseOrleans(); 
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();
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

