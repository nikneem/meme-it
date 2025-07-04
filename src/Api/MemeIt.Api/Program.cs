using MemeIt.Library.Extensions;
using MemeIt.Games.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Azure CosmosDB client with Aspire integration
builder.AddAzureCosmosClient("cosmos");
builder.AddKeyedAzureTableClient("clustering");
builder.UseOrleansClient();


// Add MemeIt Library services
builder.Services.AddMemeLibraryWithAspire();

// Add MemeIt Games services
builder.Services.AddGameServices();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
