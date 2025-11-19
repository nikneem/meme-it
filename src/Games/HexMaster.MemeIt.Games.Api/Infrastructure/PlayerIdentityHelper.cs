using System;
using Microsoft.AspNetCore.Http;

namespace HexMaster.MemeIt.Games.Api.Infrastructure;

/// <summary>
/// Utility methods for working with the player identity headers shared by the API.
/// </summary>
public static class PlayerIdentityHelper
{
    public const string PlayerIdHeaderName = "X-MemeIt-PlayerId";

    public static bool TryParsePlayerId(IHeaderDictionary headers, out Guid playerId, out string? error)
    {
        if (!headers.TryGetValue(PlayerIdHeaderName, out var values) || values.Count == 0)
        {
            playerId = Guid.Empty;
            error = "Missing X-MemeIt-PlayerId header.";
            return false;
        }

        if (!Guid.TryParse(values[0], out playerId) || playerId == Guid.Empty)
        {
            playerId = Guid.Empty;
            error = "Invalid player id supplied.";
            return false;
        }

        error = null;
        return true;
    }
}
