var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Storage emulator for Orleans
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator =>
    {
        emulator.WithDataVolume();
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });

var blobs = storage.AddBlobs("blobs");
var tables = storage.AddTables("tables");
var queues = storage.AddQueues("queues");

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

// Add Orleans cluster
var orleans = builder.AddOrleans("orleans")
    .WithClustering(tables)
    .WithGrainStorage("Default", tables)
    .WithGrainStorage("PubSubStore", tables);

// Add API project with cosmos and orleans references
builder.AddProject<Projects.MemeIt_Api>("memeit-api")
    .WithReference(cosmos)
    .WithReference(orleans)
    .WithReference(blobs)
    .WithReference(tables)
    .WithReference(queues);

builder.Build().Run();
