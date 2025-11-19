using System;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Users.Abstractions.Application.Commands;
using HexMaster.MemeIt.Users.Abstractions.Application.Users;
using HexMaster.MemeIt.Users.Abstractions.Services;

namespace HexMaster.MemeIt.Users.Application.Users;

/// <summary>
/// Handles the join user workflow by validating the supplied token and issuing a new one.
/// </summary>
public sealed class JoinUserCommandHandler : ICommandHandler<JoinUserCommand, JoinUserResult>
{
    private readonly IUserTokenService _tokenService;

    public JoinUserCommandHandler(IUserTokenService tokenService)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public Task<JoinUserResult> HandleAsync(JoinUserCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            throw new ArgumentException("Display name is required", nameof(command.DisplayName));
        }

        var sanitizedName = command.DisplayName.Trim();
        if (sanitizedName.Length is < 2 or > 32)
        {
            throw new ArgumentException("Display name must be between 2 and 32 characters", nameof(command.DisplayName));
        }

        var userId = string.IsNullOrWhiteSpace(command.AuthorizationToken)
            ? Guid.NewGuid()
            : _tokenService.ValidateToken(command.AuthorizationToken).UserId;

        var jwt = _tokenService.CreateToken(userId, sanitizedName);
        var result = new JoinUserResult(userId, sanitizedName, jwt.Token, jwt.ExpiresAt);
        return Task.FromResult(result);
    }
}
