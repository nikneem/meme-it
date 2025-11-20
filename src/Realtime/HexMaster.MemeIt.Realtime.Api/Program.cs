using HexMaster.MemeIt.Realtime.Api.Endpoints;
using HexMaster.MemeIt.Realtime.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

// Add SignalR for real-time communication
builder.Services.AddSignalR();

// Add Dapr services for pubsub integration
builder.Services.AddControllers().AddDapr();

// Configure CORS to allow SignalR connections from frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Enable CORS
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map SignalR hub endpoint
app.MapHub<GamesHub>("/hubs/games");

// Map Dapr pubsub subscription endpoints
app.MapDaprSubscriptionsEndpoints();

// Map Dapr subscribe endpoint for service discovery
app.MapSubscribeHandler();

app.Run();
