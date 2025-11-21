using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Api.Endpoints;
using HexMaster.MemeIt.Memes.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Data.Postgres;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<MemesDbContext>("memes-db");

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddMemesPostgresData(builder.Configuration);

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

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Title = "Meme-It Memes API");
}

app.UseHttpsRedirection();

// Map meme template endpoints
app.MapMemeTemplateEndpoints();

app.MapGet("/", () => Results.Ok("Meme-It Memes API"))
   .WithTags("Diagnostics")
   .WithName("GetRoot");

app.Run();
