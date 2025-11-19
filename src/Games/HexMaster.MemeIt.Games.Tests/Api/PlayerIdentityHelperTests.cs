using System;
using HexMaster.MemeIt.Games.Api.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace HexMaster.MemeIt.Games.Tests.Api;

public sealed class PlayerIdentityHelperTests
{
    [Fact]
    public void TryParsePlayerId_ReturnsTrue_WhenHeaderIsValid()
    {
        var headers = new HeaderDictionary
        {
            [PlayerIdentityHelper.PlayerIdHeaderName] = Guid.NewGuid().ToString()
        };

        var result = PlayerIdentityHelper.TryParsePlayerId(headers, out var playerId, out var error);

        Assert.True(result);
        Assert.NotEqual(Guid.Empty, playerId);
        Assert.Null(error);
    }

    [Fact]
    public void TryParsePlayerId_ReturnsFalse_WhenHeaderMissing()
    {
        var headers = new HeaderDictionary();

        var result = PlayerIdentityHelper.TryParsePlayerId(headers, out var playerId, out var error);

        Assert.False(result);
        Assert.Equal(Guid.Empty, playerId);
        Assert.Equal("Missing X-MemeIt-PlayerId header.", error);
    }

    [Fact]
    public void TryParsePlayerId_ReturnsFalse_WhenHeaderInvalid()
    {
        var headers = new HeaderDictionary
        {
            [PlayerIdentityHelper.PlayerIdHeaderName] = "not-a-guid"
        };

        var result = PlayerIdentityHelper.TryParsePlayerId(headers, out var playerId, out var error);

        Assert.False(result);
        Assert.Equal(Guid.Empty, playerId);
        Assert.Equal("Invalid player id supplied.", error);
    }
}
