using System.Collections.Generic;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.UpdateSettings;

using HexMaster.MemeIt.Games.ValueObjects;

public record UpdateSettingsCommand(string PlayerId, GameSettings Settings, string GameCode) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
}
