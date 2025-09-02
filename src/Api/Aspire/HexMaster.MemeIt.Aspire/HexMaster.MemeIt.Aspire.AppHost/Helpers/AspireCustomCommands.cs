using Aspire.Hosting.Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HexMaster.MemeIt.Aspire.AppHost.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Aspire.AppHost.Helpers;

public static class AspireCustomCommands
{

    public static IResourceBuilder<AzureBlobStorageResource> WithDataLoadCommand(this IResourceBuilder<AzureBlobStorageResource> blobs)
    {
        return blobs.WithCommand(
               name: "seed-image-blobs",
               displayName: "Seed Image Blobs",
               executeCommand: async context =>
               {
                   var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
                   try
                   {
                       var blobSeeder = new BlobSeeder(logger, AspireConstants.BlobMemesContainerName);

                       var connectionString = await blobs.Resource.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
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

                       await blobSeeder.SeedEmbeddedResourcesAsync(blobServiceClient, CancellationToken.None);
                   }
                   catch (Exception ex)
                   {
                       logger.LogError(ex, "An error occurred while seeding embedded meme resources");
                   }
                   return CommandResults.Success();
               },
               commandOptions: new CommandOptions
               {
                   IconName = "ArrowUpload",
                   IconVariant = IconVariant.Filled,
                   Description = "Seed image blobs"
               }
               );
    }

    public static IResourceBuilder<AzureCosmosDBResource> WithDataLoadCommand(this IResourceBuilder<AzureCosmosDBResource> container)
    {
        return container.WithCommand(
               name: "seed-image-templates",
               displayName: "Seed Image Templates",
               executeCommand: async context =>
               {
                   var logger = context.ServiceProvider.GetRequiredService<ILogger<MemeTemplateSeeder>>();
                   try
                   {
                       var blobSeeder = new BlobSeeder(logger, AspireConstants.BlobMemesContainerName);

                       var connectionString = await container.Resource.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
                       var cosmosClient = new CosmosClient(connectionString);
                       var cosmosDatabase = cosmosClient.GetDatabase(AspireConstants.CosmosDatabaseName);
                       var cosmosContainer = cosmosDatabase.GetContainer(AspireConstants.CosmosConfigurationContainer);

                       var seeder = new MemeTemplateSeeder(logger);
                          await seeder.SeedTemplatesAsync(cosmosContainer, context.CancellationToken);
                   }
                   catch (Exception ex)
                   {
                       logger.LogError(ex, "An error occurred while seeding embedded meme resources");
                   }
                   return CommandResults.Success();
               },
               commandOptions: new CommandOptions
               {
                   IconName = "ArrowUpload",
                   IconVariant = IconVariant.Filled,
                   Description = "Seed image templates"
               }
               );
    }
}
