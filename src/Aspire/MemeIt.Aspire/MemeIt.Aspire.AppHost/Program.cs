var builder = DistributedApplication.CreateBuilder(args);

// Add Azure CosmosDB emulator
var cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator(emulator =>
    {
        emulator.WithDataVolume();
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });

// Add database and containers
var database = cosmos.AddCosmosDatabase("memeit");
var memesContainer = database.AddContainer("memes", "/partitionKey");
var categoriesContainer = database.AddContainer("categories", "/partitionKey");

// Add API project with cosmos reference
builder.AddProject<Projects.MemeIt_Api>("memeit-api")
    .WithReference(cosmos);

builder.Build().Run();
