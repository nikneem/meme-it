using System.Collections.Generic;
using HexMaster.MemeIt.Users.Abstractions.Application.Commands;
using HexMaster.MemeIt.Users.Abstractions.Application.Users;
using HexMaster.MemeIt.Users.Abstractions.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace HexMaster.MemeIt.Users.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/users")
            .WithTags("Users");

        group.MapPost("/join", HandleJoinUser)
            .WithName("JoinUser")
            .Produces<JoinUserResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static async Task<Results<Ok<JoinUserResponse>, ValidationProblem, UnauthorizedHttpResult>> HandleJoinUser(
        JoinUserRequest request,
        HttpRequest httpRequest,
        ICommandHandler<JoinUserCommand, JoinUserResult> handler,
        CancellationToken cancellationToken)
    {
        var authorizationToken = ExtractBearerToken(httpRequest);

        var command = new JoinUserCommand(request.DisplayName, authorizationToken);
        JoinUserResult result;
        try
        {
            result = await handler.HandleAsync(command, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                { ex.ParamName ?? "displayName", new[] { ex.Message } }
            });
        }
        catch (SecurityTokenException)
        {
            return TypedResults.Unauthorized();
        }

        var response = new JoinUserResponse(result.UserId, result.DisplayName, result.Token, result.ExpiresAt);
        return TypedResults.Ok(response);
    }

    private static string? ExtractBearerToken(HttpRequest httpRequest)
    {
        if (!httpRequest.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
        {
            return null;
        }

        var headerValue = authorizationHeader.ToString();
        const string bearerPrefix = "Bearer ";
        return headerValue.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? headerValue[bearerPrefix.Length..].Trim()
            : null;
    }
}
