using Aspire.Hosting;
using Azure.Provisioning;
using Azure.Provisioning.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using HexMaster.MemeIt.Aspire;
using HexMaster.MemeIt.Aspire.AppHost.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis(AspireConstants.RedisCacheName, 64820)
    .WithLifetime(ContainerLifetime.Session);

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
var blobs = storage.AddBlobs(AspireConstants.BlobServiceName);
storage.AddBlobContainer(AspireConstants.BlobUploadContainerName, AspireConstants.BlobServiceName);
var memesContainer = storage.AddBlobContainer(AspireConstants.BlobMemesContainerName, AspireConstants.BlobServiceName);

blobs.OnResourceReady(async (resource, @event, cancellationToken) =>
{
    var logger = @event.Services.GetRequiredService<ILogger<Program>>();
    var blobSeeder = new BlobSeeder(logger, AspireConstants.BlobMemesContainerName);

    try
    {
        var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken);
        var blobServiceClient = new BlobServiceClient(connectionString);

        // Ensure the container exists
        try
        {
            var containerClient = await blobServiceClient.CreateBlobContainerAsync(AspireConstants.BlobMemesContainerName, PublicAccessType.Blob);
        }
        catch
        {
            logger.LogInformation("Blob container '{ContainerName}' already exists.", AspireConstants.BlobMemesContainerName);
        }

        await blobSeeder.SeedEmbeddedResourcesAsync(blobServiceClient, cancellationToken);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding embedded meme resources");
    }
});

// Add Azure CosmosDB (with emulator for development)
var cosmos = builder.AddAzureCosmosDB(AspireConstants.CosmosConnection);
if (builder.Environment.IsDevelopment())
{
    cosmos.RunAsEmulator(emulator =>
    {
        emulator.WithLifetime(ContainerLifetime.Persistent);
        emulator.WithDataVolume();
    });
}
var memesDatabase = cosmos.AddCosmosDatabase(AspireConstants.CosmosDatabaseName);
var memeTemplatesContainer = memesDatabase.AddContainer(AspireConstants.CosmosConfigurationContainer, "/partitionKey");

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
