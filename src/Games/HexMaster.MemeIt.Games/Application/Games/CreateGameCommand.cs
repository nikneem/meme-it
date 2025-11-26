using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Command issued when a player creates a new game.
/// </summary>
/// <param name="PlayerId">Identifier of the player issuing the request.</param>
/// <param name="PlayerDisplayName">Display name used for the admin player.</param>
/// <param name="Password">Optional password protecting the lobby.</param>
public sealed record CreateGameCommand(Guid PlayerId, string PlayerDisplayName, string? Password)
    : ICommand<CreateGameResult>;
