using System.Collections.Generic;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Games.Features.UpdateSettings;

public record UpdateSettingsCommand(string PlayerId, Dictionary<string, string> Settings, string GameCode) : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
}
