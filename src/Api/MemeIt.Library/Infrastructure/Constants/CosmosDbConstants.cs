namespace MemeIt.Library.Infrastructure.Constants;

/// <summary>
/// Constants for CosmosDB operations
/// </summary>
public static class CosmosDbConstants
{
    /// <summary>
    /// Document types
    /// </summary>
    public static class DocumentTypes
    {
        public const string Meme = "meme";
        public const string Category = "category";
    }

    /// <summary>
    /// Partition key values
    /// </summary>
    public static class PartitionKeys
    {
        public const string Category = "category";
        public const string Default = "default";
    }

    /// <summary>
    /// Field names used in queries
    /// </summary>
    public static class FieldNames
    {
        public const string Id = "id";
        public const string Type = "type";
        public const string IsActive = "isActive";
        public const string Categories = "categories";
        public const string Category = "category";
        public const string DisplayOrder = "displayOrder";
        public const string Name = "name";
        public const string PartitionKey = "partitionKey";
        public const string Title = "title";
        public const string ImageUrl = "imageUrl";
        public const string CreatedAt = "createdAt";
        public const string UpdatedAt = "updatedAt";
        public const string PopularityScore = "popularityScore";
    }

    /// <summary>
    /// Query parameter names
    /// </summary>
    public static class Parameters
    {
        public const string Id = "id";
        public const string Type = "type";
        public const string IsActive = "isActive";
        public const string Category = "category";
        public const string Categories = "categories";
        public const string ExcludedIds = "excludedIds";
        public const string Exclude = "exclude";
        public const string Ids = "ids";
        public const string Increment = "increment";
    }
}
