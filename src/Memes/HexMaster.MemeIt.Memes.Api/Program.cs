using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Abstractions.Services;
using HexMaster.MemeIt.Memes.Api.Endpoints;
using HexMaster.MemeIt.Memes.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Data.Postgres;
using HexMaster.MemeIt.Memes.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<MemesDbContext>("memes-db");
builder.AddAzureBlobServiceClient("memes-blobs");

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Authorization")
              .AllowCredentials();
    });
});

// Register repositories (DbContext is already registered by Aspire)
builder.Services.AddScoped<HexMaster.MemeIt.Memes.Repositories.IMemeTemplateRepository, PostgresMemeTemplateRepository>();

// Register services
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

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

// Apply migrations automatically in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MemesDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Title = "Meme-It Memes API");
}

app.UseCors("AllowAngularApp");
app.UseHttpsRedirection();

// Map meme template endpoints
app.MapMemeTemplateEndpoints();

app.MapGet("/", () => Results.Ok("Meme-It Memes API"))
   .WithTags("Diagnostics")
   .WithName("GetRoot");

app.Run();
