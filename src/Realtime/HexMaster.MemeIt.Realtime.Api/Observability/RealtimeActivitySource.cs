using System.Diagnostics;

namespace HexMaster.MemeIt.Realtime.Api.Observability;

public static class RealtimeActivitySource
{
    public const string SourceName = "HexMaster.MemeIt.Realtime";

    public static readonly ActivitySource Instance = new(SourceName, "1.0.0");
}
