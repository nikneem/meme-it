using HexMaster.MemeIt.Realtime.Api.Endpoints;
using HexMaster.MemeIt.Realtime.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

// Add SignalR for real-time communication with camelCase JSON serialization
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add Dapr services for pubsub integration
builder.Services.AddControllers();

builder.Services.AddDaprClient();

// Configure CORS to allow SignalR connections from frontend and gateway
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",  // Angular dev server
                "http://localhost:5000")  // Aspire gateway
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
app.UseCloudEvents();
// Map Dapr subscribe endpoint for service discovery
app.MapSubscribeHandler();
// The local endpoints in this API the make the subscriptions work
app.MapDaprSubscriptionsEndpoints();

app.Run();
