using Aspire.Hosting;
using Azure.Provisioning;
using Azure.Provisioning.Storage;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis(AspireConstants.RedisCacheName, 64820)
    .WithLifetime(ContainerLifetime.Persistent);

var orleans = builder.AddOrleans(AspireConstants.MemeItOrleansCluster)
    .WithClustering(redis)
    .WithGrainStorage("games", redis);

// Add Azure Storage (with Azurite emulator for development)
var storage = builder.AddAzureStorage("Storage").ConfigureInfrastructure(infra =>
{
    var blobStorage = infra.GetProvisionableResources().OfType<BlobService>().Single();

    blobStorage.CorsRules.Add(new BicepValue<StorageCorsRule>(new StorageCorsRule
    {
        AllowedOrigins = [new BicepValue<string>("http://localhost:4200")],
        AllowedMethods = [CorsRuleAllowedMethod.Get, CorsRuleAllowedMethod.Put, CorsRuleAllowedMethod.Options],
        AllowedHeaders = [new BicepValue<string>("*")],
        ExposedHeaders = [new BicepValue<string>("*")],
        MaxAgeInSeconds = new BicepValue<int>(3600)
    }));

});
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

var api = builder.AddProject<Projects.HexMaster_MemeIt_Api>(AspireConstants.MemeItApiProjectName)
    .WithReference(orleans)
    .WithReference(blobs)
    .WithReference(memeTemplatesContainer)
    .WaitFor(redis)
    .WaitFor(storage)
    .WaitFor(cosmos)
    .WithReplicas(2)
    .WithExternalHttpEndpoints();

// Add Angular frontend application
var frontEndSourceFolder = Path.GetFullPath(builder.AppHostDirectory + "../../../../../Web");
if (Directory.Exists(frontEndSourceFolder))
{
    var frontend = builder.AddNpmApp("Frontend", frontEndSourceFolder)
        .WaitFor(api)
            .WithHttpEndpoint(isProxied: false, port: 4200)
            .WithHttpHealthCheck();
}

builder.Build().Run();
