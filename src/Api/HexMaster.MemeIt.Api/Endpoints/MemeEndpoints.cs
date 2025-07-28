using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using HexMaster.MemeIt.Memes.Features.CreateMeme;
using HexMaster.MemeIt.Memes.Features.DeleteMeme;
using HexMaster.MemeIt.Memes.Features.GenerateUploadSas;
using HexMaster.MemeIt.Memes.Features.GetMeme;
using HexMaster.MemeIt.Memes.Features.ListMemes;
using HexMaster.MemeIt.Memes.Features.UpdateMeme;
using Localizr.Core.Abstractions.Cqrs;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.MemeIt.Api.Endpoints;

public static class MemeEndpoints
{
    public static void MapMemeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/management/memes").WithTags("Meme Management");

        group.MapGet("/", ListMemes)
            .WithName(nameof(ListMemes))
            .Produces<IEnumerable<MemeTemplateListResponse>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateMeme)
            .WithName(nameof(CreateMeme))
            .Produces<CreateMemeResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id}", GetMeme)
            .WithName(nameof(GetMeme))
            .Produces<MemeTemplateResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id}", UpdateMeme)
            .WithName(nameof(UpdateMeme))
            .Produces<MemeTemplateResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id}", DeleteMeme)
            .WithName(nameof(DeleteMeme))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/upload", GenerateUploadSas)
            .WithName(nameof(GenerateUploadSas))
            .Produces<HexMaster.MemeIt.Memes.Features.GenerateUploadSas.GenerateUploadSasResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> ListMemes(
        [FromServices] IQueryHandler<ListMemesQuery, IEnumerable<MemeTemplateListResponse>> handler)
    {
        var query = new ListMemesQuery();
        var response = await handler.HandleAsync(query, CancellationToken.None);
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateMeme(
        [FromBody] CreateMemeRequest request,
        [FromServices] ICommandHandler<CreateMemeCommand, CreateMemeResponse> handler)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(request.Name), ["Name is required."] }
            });
        }

        if (string.IsNullOrWhiteSpace(request.SourceImage))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(request.SourceImage), ["Source image is required."] }
            });
        }

        if (request.SourceWidth <= 0 || request.SourceHeight <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(request.SourceWidth), ["Source width must be greater than 0."] },
                { nameof(request.SourceHeight), ["Source height must be greater than 0."] }
            });
        }

        var command = new CreateMemeCommand
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            SourceImage = request.SourceImage,
            SourceWidth = request.SourceWidth,
            SourceHeight = request.SourceHeight,
            TextAreas = request.Textareas ?? Array.Empty<MemeTextArea>()
        };

        try
        {
            var response = await handler.HandleAsync(command, CancellationToken.None);
            return Results.Created($"/management/memes/{response.Id}", response);
        }
        catch (FileNotFoundException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(request.SourceImage), [ex.Message] }
            });
        }
    }

    private static async Task<IResult> GetMeme(
        [AsParameters] GetMemeQuery query,
        [FromServices] IQueryHandler<GetMemeQuery, OperationResult<MemeTemplateResponse>> handler)
    {
        var response = await handler.HandleAsync(query, CancellationToken.None);
        
        if (response.Success)
        {
            return Results.Ok(response.ResponseObject);
        }
        
        return Results.NotFound();
    }

    private static async Task<IResult> UpdateMeme(
        [FromRoute] string id,
        [FromBody] UpdateMemeRequest request,
        [FromServices] ICommandHandler<UpdateMemeCommand, OperationResult<MemeTemplateResponse>> handler)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(request.Name), ["Name is required."] }
            });
        }

        var command = new UpdateMemeCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            TextAreas = request.TextAreas ?? Array.Empty<MemeTextArea>()
        };

        var response = await handler.HandleAsync(command, CancellationToken.None);
        
        if (response.Success)
        {
            return Results.Ok(response.ResponseObject);
        }
        
        return Results.NotFound();
    }

    private static async Task<IResult> DeleteMeme(
        [FromRoute] string id,
        [FromServices] ICommandHandler<DeleteMemeCommand, OperationResult<object>> handler)
    {
        var command = new DeleteMemeCommand { Id = id };
        var response = await handler.HandleAsync(command, CancellationToken.None);
        
        if (response.Success)
        {
            return Results.NoContent();
        }
        
        return Results.NotFound();
    }

    private static async Task<IResult> GenerateUploadSas(
        [FromBody] GenerateUploadSasCommand command,
        [FromServices] ICommandHandler<GenerateUploadSasCommand, HexMaster.MemeIt.Memes.Features.GenerateUploadSas.GenerateUploadSasResponse> handler)
    {
        if (string.IsNullOrWhiteSpace(command.FileName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { nameof(command.FileName), ["FileName is required."] }
            });
        }

        try
        {
            var response = await handler.HandleAsync(command, CancellationToken.None);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
