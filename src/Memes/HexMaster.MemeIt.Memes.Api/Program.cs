using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Configuration;
using HexMaster.MemeIt.Memes.Abstractions.Services;
using HexMaster.MemeIt.Memes.Api.Endpoints;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.CreateMemeTemplate;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.DeleteMemeTemplate;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.GenerateUploadSasToken;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.GetMemeTemplateById;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.GetRandomMemeTemplate;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.ListMemeTemplates;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.UpdateMemeTemplate;
using HexMaster.MemeIt.Memes.Application.Observability;
using HexMaster.MemeIt.Memes.Data.Postgres;
using HexMaster.MemeIt.Memes.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure OpenTelemetry with custom activity sources and meters
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(MemesActivitySource.SourceName))
    .WithMetrics(metrics => metrics
        .AddMeter("HexMaster.MemeIt.Memes"));

builder.AddNpgsqlDbContext<MemesDbContext>("memes-db");
builder.AddAzureBlobServiceClient("memes-blobs");

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://memeit.hexmaster.nl")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Authorization", "X-ApiKey")
              .AllowCredentials();
    });
});

// Register configuration
builder.Services.Configure<BlobStorageOptions>(
    builder.Configuration.GetSection(BlobStorageOptions.SectionName));

// Register repositories (DbContext is already registered by Aspire)
builder.Services.AddScoped<HexMaster.MemeIt.Memes.Repositories.IMemeTemplateRepository, PostgresMemeTemplateRepository>();

// Register services
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<MemesMetrics>();

// Register command handlers
builder.Services.AddScoped<ICommandHandler<CreateMemeTemplateCommand, CreateMemeTemplateResult>, CreateMemeTemplateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateMemeTemplateCommand, UpdateMemeTemplateResult>, UpdateMemeTemplateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteMemeTemplateCommand, DeleteMemeTemplateResult>, DeleteMemeTemplateCommandHandler>();

// Register query handlers
builder.Services.AddScoped<IQueryHandler<GetRandomMemeTemplateQuery, GetRandomMemeTemplateResult>, GetRandomMemeTemplateQueryHandler>();
builder.Services.AddScoped<IQueryHandler<ListMemeTemplatesQuery, ListMemeTemplatesResult>, ListMemeTemplatesQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetMemeTemplateByIdQuery, GetMemeTemplateByIdResult>, GetMemeTemplateByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GenerateUploadSasTokenQuery, GenerateUploadSasTokenResult>, GenerateUploadSasTokenQueryHandler>();

var app = builder.Build();

// Check if running in migration mode
var runMigrations = app.Environment.IsDevelopment() ||
                    args.Contains("--migrate") ||
                    builder.Configuration.GetValue<bool>("RunDatabaseMigrations");

if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MemesDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Running database migrations...");
    dbContext.Database.EnsureCreated();
    logger.LogInformation("Database migrations completed successfully");

    // Exit after migrations if --migrate flag was used
    if (args.Contains("--migrate"))
    {
        logger.LogInformation("Migration-only mode: exiting after database update");
        return;
    }
}

app.MapDefaultEndpoints();
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Title = "Meme-It Memes Management API");
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");



// Map meme template endpoints
app.MapMemeTemplateEndpoints();

app.MapGet("/", () => Results.Ok("Meme-It Memes API"))
   .WithTags("Diagnostics")
   .WithName("GetRoot");

app.Run();