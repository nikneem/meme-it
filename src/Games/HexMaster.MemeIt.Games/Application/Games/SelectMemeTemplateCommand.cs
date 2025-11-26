using HexMaster.MemeIt.Games.Abstractions.Application.Commands;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Command issued when a player selects a meme template for the current round.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="PlayerId">The player selecting the meme.</param>
/// <param name="RoundNumber">The round number.</param>
/// <param name="MemeTemplateId">The ID of the selected meme template.</param>
public sealed record SelectMemeTemplateCommand(string GameCode, Guid PlayerId, int RoundNumber, Guid MemeTemplateId)
    : ICommand<SelectMemeTemplateResult>;
