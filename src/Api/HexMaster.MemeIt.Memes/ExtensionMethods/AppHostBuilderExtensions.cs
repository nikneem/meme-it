using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using HexMaster.MemeIt.Memes.Features.CreateMeme;
using HexMaster.MemeIt.Memes.Features.DeleteMeme;
using HexMaster.MemeIt.Memes.Features.GetMeme;
using HexMaster.MemeIt.Memes.Features.ListMemes;
using HexMaster.MemeIt.Memes.Features.GenerateUploadSas;
using HexMaster.MemeIt.Memes.Features.UpdateMeme;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Aspire;
using HexMaster.MemeIt.Memes.Services;
using Localizr.Core.Abstractions.Cqrs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HexMaster.MemeIt.Memes.ExtensionMethods;

public static class AppHostBuilderExtensions
{
    public static IHostApplicationBuilder AddMemesServices(this IHostApplicationBuilder builder)
    {
        // Register Aspire integrations for Azure services
        builder.AddAzureBlobServiceClient(AspireConstants.BlobServiceName);
        
        // Configure CosmosDB with camelCase serialization
        builder.AddAzureCosmosContainer(AspireConstants.CosmosConfigurationContainer, configureClientOptions: options =>
        {
            options.SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };
        });

        // Register repositories and services
        builder.Services.AddScoped<IMemeTemplateRepository, MemeTemplateRepository>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<ISasTokenService, SasTokenService>();
        builder.Services.AddScoped<MemeTemplateSeeder>();
        builder.Services.AddScoped<IBlobUrlService, BlobUrlService>();

        // Register background services
        builder.Services.AddHostedService<UploadCleanupService>();

        // Register command handlers
        builder.Services.AddTransient<ICommandHandler<CreateMemeCommand, CreateMemeResponse>, CreateMemeCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<GenerateUploadSasCommand, Features.GenerateUploadSas.GenerateUploadSasResponse>, GenerateUploadSasCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<UpdateMemeCommand, OperationResult<MemeTemplateResponse>>, UpdateMemeCommandHandler>();
        builder.Services.AddTransient<ICommandHandler<DeleteMemeCommand, OperationResult<object>>, DeleteMemeCommandHandler>();

        // Register query handlers
        builder.Services.AddTransient<IQueryHandler<GetMemeQuery, OperationResult<MemeTemplateResponse>>, GetMemeQueryHandler>();
        builder.Services.AddTransient<IQueryHandler<ListMemesQuery, IEnumerable<MemeTemplateListResponse>>, ListMemesQueryHandler>();

        return builder;
    }
}
