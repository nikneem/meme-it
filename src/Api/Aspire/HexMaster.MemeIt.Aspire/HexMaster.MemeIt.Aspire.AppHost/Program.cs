using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Azure.Provisioning;
using Azure.Provisioning.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using HexMaster.MemeIt.Aspire;
using HexMaster.MemeIt.Aspire.AppHost.Helpers;
using HexMaster.MemeIt.Aspire.AppHost.Services;
using Humanizer.Localisation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;

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
blobs.WithDataLoadCommand();



// Add Azure CosmosDB (with emulator for development)
var cosmos = builder.AddAzureCosmosDB(AspireConstants.CosmosConnection)
    .WithDataLoadCommand();

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
//api.WithCommand(
//    name: "seed-data",
//    displayName: "Seed initial data",
//    executeCommand: async context => {

////        api.Resource.GetEndpoints

//        var container = context.ServiceProvider.GetRequiredService<Container>();
//        var logger = context.ServiceProvider.GetRequiredService<ILogger<MemeTemplateSeeder>>();
//        var seeder = new MemeTemplateSeeder(logger);
//        await seeder.SeedTemplatesAsync(container, context.CancellationToken);

//        //        var x = HexMaster.MemeIt.


//        return CommandResults.Success();
//    },
//commandOptions: new CommandOptions
//{
//    IconName = "DrawerArrowDownload",
//    IconVariant = IconVariant.Filled,
//    Description = "Seeds the database with initial meme templates",
//}

//    );



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
