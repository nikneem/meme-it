var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Storage emulator for Orleans
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator =>
    {
        emulator.WithDataVolume();
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });

var grainStorage = storage.AddBlobs("grain-state");
var clusteringTable = storage.AddTables("clustering");

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
    .WithClustering(clusteringTable)
    .WithGrainStorage("Default", grainStorage );

builder.AddProject<Projects.MemeIt_OrleansServer>("silo")
    .WithReference(orleans)
    .WithReplicas(3);



// Add API project with cosmos and orleans references
builder.AddProject<Projects.MemeIt_Api>("memeit-api")
    .WithReference(cosmos)
    .WithReference(orleans.AsClient())
    .WithReference(grainStorage )
    .WithReference(clusteringTable);

var frontEndSourceFolder = Path.GetFullPath(builder.AppHostDirectory + "/../../../App");
if (Directory.Exists(frontEndSourceFolder))
{
    builder.AddNpmApp("frontend", frontEndSourceFolder)
        .WithHttpEndpoint(isProxied: false, port: 4200)
        .WithHttpHealthCheck();
    //        .WithNpmPackageInstallation();
}

builder.Build().Run();
