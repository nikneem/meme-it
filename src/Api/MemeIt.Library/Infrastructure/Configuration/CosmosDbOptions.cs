namespace MemeIt.Library.Infrastructure.Configuration;

/// <summary>
/// Configuration options for CosmosDB
/// </summary>
public class CosmosDbOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "CosmosDb";

    /// <summary>
    /// Gets or sets the CosmosDB connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name
    /// </summary>
    public string DatabaseName { get; set; } = "memeit";

    /// <summary>
    /// Gets or sets the memes container name
    /// </summary>
    public string MemesContainerName { get; set; } = "memes";

    /// <summary>
    /// Gets or sets the categories container name
    /// </summary>
    public string CategoriesContainerName { get; set; } = "categories";

    /// <summary>
    /// Gets or sets the maximum retry attempts for CosmosDB operations
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum retry wait time in seconds
    /// </summary>
    public int MaxRetryWaitTimeInSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to create the database and containers if they don't exist
    /// </summary>
    public bool CreateIfNotExists { get; set; } = true;

    /// <summary>
    /// Gets or sets the request units per second for the memes container
    /// </summary>
    public int MemesContainerThroughput { get; set; } = 400;

    /// <summary>
    /// Gets or sets the request units per second for the categories container
    /// </summary>
    public int CategoriesContainerThroughput { get; set; } = 400;
}
