using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Api.Requests;

namespace HexMaster.MemeIt.Memes.Api.Endpoints;

/// <summary>
/// Endpoint definitions for meme template management.
/// </summary>
public static class MemeTemplateEndpoints
{
    public static IEndpointRouteBuilder MapMemeTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memes")
            .WithTags("Meme Templates")
            .WithOpenApi();

        // Admin endpoints for managing templates
        group.MapPost("/templates", async (
            CreateMemeTemplateRequest request,
            ICommandHandler<CreateMemeTemplateCommand, CreateMemeTemplateResult> handler,
            CancellationToken ct) =>
        {
            try
            {
                var command = new CreateMemeTemplateCommand(
                    request.Title,
                    request.ImageUrl,
                    request.TextAreas
                );

                var result = await handler.HandleAsync(command, ct);
                return Results.Created($"/api/memes/templates/{result.Id}", new { id = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateMemeTemplate")
        .WithSummary("Create a new meme template (Admin)")
        .Produces(201)
        .Produces(400);

        group.MapPut("/templates/{id:guid}", async (
            Guid id,
            UpdateMemeTemplateRequest request,
            ICommandHandler<UpdateMemeTemplateCommand, UpdateMemeTemplateResult> handler,
            CancellationToken ct) =>
        {
            try
            {
                var command = new UpdateMemeTemplateCommand(
                    id,
                    request.Title,
                    request.ImageUrl,
                    request.TextAreas
                );

                var result = await handler.HandleAsync(command, ct);
                return result.Success
                    ? Results.NoContent()
                    : Results.NotFound(new { error = "Template not found" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateMemeTemplate")
        .WithSummary("Update an existing meme template (Admin)")
        .Produces(204)
        .Produces(400)
        .Produces(404);

        group.MapDelete("/templates/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteMemeTemplateCommand, DeleteMemeTemplateResult> handler,
            CancellationToken ct) =>
        {
            var command = new DeleteMemeTemplateCommand(id);
            var result = await handler.HandleAsync(command, ct);

            return result.Success
                ? Results.NoContent()
                : Results.NotFound(new { error = "Template not found" });
        })
        .WithName("DeleteMemeTemplate")
        .WithSummary("Delete a meme template (Admin)")
        .Produces(204)
        .Produces(404);

        group.MapGet("/templates", async (
            IQueryHandler<ListMemeTemplatesQuery, ListMemeTemplatesResult> handler,
            CancellationToken ct) =>
        {
            var query = new ListMemeTemplatesQuery();
            var result = await handler.HandleAsync(query, ct);
            return Results.Ok(result.Templates);
        })
        .WithName("ListMemeTemplates")
        .WithSummary("List all meme templates (Admin)")
        .Produces(200);

        group.MapGet("/templates/{id:guid}", async (
            Guid id,
            IQueryHandler<GetMemeTemplateByIdQuery, GetMemeTemplateByIdResult> handler,
            CancellationToken ct) =>
        {
            var query = new GetMemeTemplateByIdQuery(id);
            var result = await handler.HandleAsync(query, ct);

            return result.Template is not null
                ? Results.Ok(result.Template)
                : Results.NotFound(new { error = "Template not found" });
        })
        .WithName("GetMemeTemplateById")
        .WithSummary("Get a meme template by ID")
        .Produces(200)
        .Produces(404);

        // Player endpoint for getting a random template
        group.MapGet("/random", async (
            IQueryHandler<GetRandomMemeTemplateQuery, GetRandomMemeTemplateResult> handler,
            CancellationToken ct) =>
        {
            var query = new GetRandomMemeTemplateQuery();
            var result = await handler.HandleAsync(query, ct);

            return result.Template is not null
                ? Results.Ok(result.Template)
                : Results.NotFound(new { error = "No templates available" });
        })
        .WithName("GetRandomMemeTemplate")
        .WithSummary("Get a random meme template for gameplay")
        .Produces(200)
        .Produces(404);

        group.MapPost("/upload-token", async (
            IQueryHandler<GenerateUploadSasTokenQuery, GenerateUploadSasTokenResult> handler,
            CancellationToken ct) =>
        {
            var query = new GenerateUploadSasTokenQuery();
            var result = await handler.HandleAsync(query, ct);
            return Results.Ok(result);
        })
        .WithName("GenerateUploadSasToken")
        .WithSummary("Generate a SAS token for uploading meme template images")
        .Produces(200);

        return app;
    }
}
