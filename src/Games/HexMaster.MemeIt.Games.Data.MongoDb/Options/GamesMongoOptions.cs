namespace HexMaster.MemeIt.Games.Data.MongoDb.Options;

/// <summary>
/// Configuration for Mongo persistence in the games module.
/// </summary>
public sealed class GamesMongoOptions
{
    public const string SectionName = "Games:Mongo";
    public const string DefaultDatabaseName = "games-db";
    public const string DefaultCollectionName = "games";

    public string DatabaseName { get; set; } = DefaultDatabaseName;

    public string CollectionName { get; set; } = DefaultCollectionName;
}
