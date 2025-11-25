using System;
using System.Collections.Generic;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.IntegrationEvents.Events;

namespace HexMaster.MemeIt.Games.Application.Games;

public sealed record SubmitMemeCommand(
    string GameCode,
    int RoundNumber,
    Guid PlayerId,
    Guid MemeTemplateId,
    IReadOnlyCollection<MemeTextEntryDto> TextEntries)
    : ICommand<SubmitMemeResult>;
