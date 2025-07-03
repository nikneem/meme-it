using MemeIt.Library.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddOrleansDefaults();

// Add Azure CosmosDB client with Aspire integration
builder.AddAzureCosmosClient("cosmos");

// Add Azure Storage services with Aspire integration
builder.AddAzureBlobClient("blobs");
builder.AddAzureQueueClient("queues");

// Add Orleans with Aspire integration
builder.UseOrleans();

// Add MemeIt Library services
builder.Services.AddMemeLibraryWithAspire();

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
