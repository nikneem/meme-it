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

var app = builder.Build();

app.MapDefaultEndpoints();

// Use CORS middleware
app.UseCors();

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

