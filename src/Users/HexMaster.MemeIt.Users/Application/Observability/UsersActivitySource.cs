using System.Diagnostics;

namespace HexMaster.MemeIt.Users.Application.Observability;

public static class UsersActivitySource
{
    public const string SourceName = "HexMaster.MemeIt.Users";

    public static readonly ActivitySource Instance = new(SourceName, "1.0.0");
}
