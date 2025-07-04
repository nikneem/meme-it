var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureBlobClient("grain-state");
builder.UseOrleans();

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapGet("/", () => "OK");
await app.RunAsync();
