using System.Diagnostics;

namespace HexMaster.MemeIt.Memes.Application.Observability;

public static class MemesActivitySource
{
    public const string SourceName = "HexMaster.MemeIt.Memes";

    public static readonly ActivitySource Instance = new(SourceName, "1.0.0");
}
