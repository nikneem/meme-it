using HexMaster.MemeIt.Users.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Users.Abstractions.Application.Users;

/// <summary>
/// Command that requests a player name to be registered (or updated) in the Users service.
/// </summary>
/// <param name="DisplayName">Desired public player name.</param>
/// <param name="AuthorizationToken">Optional bearer token supplied by the caller.</param>
public sealed record JoinUserCommand(string DisplayName, string? AuthorizationToken)
    : ICommand<JoinUserResult>;
