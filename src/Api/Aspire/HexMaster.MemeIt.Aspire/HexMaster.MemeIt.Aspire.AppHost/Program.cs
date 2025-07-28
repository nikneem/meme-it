using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis(AspireConstants.RedisCacheName, 64820)
    .WithLifetime(ContainerLifetime.Persistent);

var orleans = builder.AddOrleans(AspireConstants.MemeItOrleansCluster)
    .WithClustering(redis)
    .WithGrainStorage("games", redis);

// Add Azure Storage (with Azurite emulator for development)
var storage = builder.AddAzureStorage("Storage");
if (builder.Environment.IsDevelopment())
{
    storage.RunAsEmulator(em =>
    {
        em.WithLifetime(ContainerLifetime.Persistent);
    });
}
var blobs = storage.AddBlobs("BlobConnection");
blobs.AddBlobContainer("upload");
blobs.AddBlobContainer("memes");

// Add Azure CosmosDB (with emulator for development)
var cosmos = builder.AddAzureCosmosDB("CosmosConnection");
if (builder.Environment.IsDevelopment())
{
    cosmos.RunAsEmulator(emulator =>
    {
        emulator.WithLifetime(ContainerLifetime.Persistent);
        emulator.WithDataVolume();
    });
}
var memesDatabase = cosmos.AddCosmosDatabase("MemeItDatabase");
var memeTemplatesContainer = memesDatabase.AddContainer("MemeTemplates", "/partitionKey");

builder.AddProject<Projects.HexMaster_MemeIt_Api>(AspireConstants.MemeItApiProjectName)
    .WithReference(orleans)
    .WithReference(blobs)
    .WithReference(memeTemplatesContainer)
    .WaitFor(redis)
    .WithReplicas(2)
    .WithExternalHttpEndpoints();

builder.Build().Run();
