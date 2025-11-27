using System.Diagnostics;

namespace HexMaster.MemeIt.Games.Application.Observability;

public static class GamesActivitySource
{
    public const string SourceName = "HexMaster.MemeIt.Games";

    public static readonly ActivitySource Instance = new(SourceName, "1.0.0");
}
