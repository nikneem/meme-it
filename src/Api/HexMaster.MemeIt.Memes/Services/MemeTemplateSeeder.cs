using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using HexMaster.MemeIt.Memes.Models.Entities;

namespace HexMaster.MemeIt.Memes.Services;

public class MemeTemplateSeeder
{
    private readonly ILogger<MemeTemplateSeeder> _logger;

    public MemeTemplateSeeder(ILogger<MemeTemplateSeeder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedTemplatesAsync(Container cosmosContainer, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting meme template seeding process...");


            // Check if any meme templates already exist
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.partitionKey = @partitionKey")
                .WithParameter("@partitionKey", MemesConstants.CosmosDbPartitionKey);

            using var iterator = cosmosContainer.GetItemQueryIterator<int>(query);
            var response = await iterator.ReadNextAsync(cancellationToken);
            var count = response.FirstOrDefault();

            if (count > 0)
            {
                _logger.LogInformation("Meme templates already exist ({Count} found). Skipping seeding.", count);
                return;
            }

            _logger.LogInformation("No meme templates found. Starting seeding process...");

            // Seed the Obama template
            var obamaTemplate = ObamaTemplate.Create();
            await cosmosContainer.UpsertItemAsync(obamaTemplate, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully seeded Obama meme template with ID: {TemplateId}", obamaTemplate.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding meme templates");
            throw;
        }
    }
}

internal static class ObamaTemplate
{
    public static MemeTemplateEntity Create()
    {
        return new MemeTemplateEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001").ToString(),
            Name = "Obama",
            Description = "Barack Obama pointing to Barack Obama",
            SourceHeight = 627,
            SourceWidth = 1015,
            CreatedAt = DateTimeOffset.UtcNow,
            SourceImageUrl = "obama.jpg", // Store only filename, not full URL
            PartitionKey = MemesConstants.CosmosDbPartitionKey,
            TextAreas = [
                new TextAreaEntity
                {
                    X = 15,
                    Y = 15,
                    Width = 771,
                    Height = 91,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 48,
                    FontColor = "#FFFFFF",
                    FontBold = true,
                    MaxLength = 100,
                    BorderThickness = 2,
                    BorderColor = "#000000"
                },
                new TextAreaEntity
                {
                    X = 15,
                    Y = 376,
                    Width = 771,
                    Height = 95,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 32,
                    FontColor = "#FFFFFF",
                    FontBold = true,
                    MaxLength = 100,
                    BorderThickness = 1,
                    BorderColor = "#000000"
                },
            ]
        };
    }
}