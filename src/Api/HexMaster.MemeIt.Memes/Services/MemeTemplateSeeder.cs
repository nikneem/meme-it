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

            // Seed the Obama template
            var obamaTemplate = ObamaTemplate.Create();
            var wallTalk = WallTalk.Create();
            var cartoon = Cartoon.Create();
            var alwaysHasBeen = AlwaysHasBeen.Create();
            var bernieSanders = BernieSanders.Create();
            var elfsTalking = ElfsTalking.Create();
            var handsUp = HandsUp.Create();
            await cosmosContainer.UpsertItemAsync(obamaTemplate, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);
            await cosmosContainer.UpsertItemAsync(wallTalk, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);
            await cosmosContainer.UpsertItemAsync(cartoon, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);
            await cosmosContainer.UpsertItemAsync(alwaysHasBeen, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);
            await cosmosContainer.UpsertItemAsync(bernieSanders, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);
            await cosmosContainer.UpsertItemAsync(elfsTalking, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);
            await cosmosContainer.UpsertItemAsync(handsUp, new PartitionKey(obamaTemplate.PartitionKey), cancellationToken: cancellationToken);


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

internal static class WallTalk
{
    public static MemeTemplateEntity Create()
    {
        return new MemeTemplateEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002").ToString(),
            Name = "Walltalk",
            Description = "Man talking to a wall",
            SourceHeight = 360,
            SourceWidth = 640,
            CreatedAt = DateTimeOffset.UtcNow,
            SourceImageUrl = "talking-to-wall.mp4", // Store only filename, not full URL
            PartitionKey = MemesConstants.CosmosDbPartitionKey,
            TextAreas = [
                new TextAreaEntity
                {
                    X = 5,
                    Y = 8,
                    Width = 625,
                    Height = 76,
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
                    X = 7,
                    Y = 256,
                    Width = 622,
                    Height = 81,
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

internal static class Cartoon
{
    // JSON: {"id":"6a08df96-fe68-4e21-9761-a7d749443b77","name":"Cartoon","description":"Cartoon getting in a dark version of itself","sourceImageUrl":"http://127.0.0.1:61433/devstoreaccount1/memes/f9c5f228-f339-4589-8e10-2b7dafd07774.jpg","sourceWidth":1600,"sourceHeight":900,"textAreas":[{"x":10,"y":310,"width":374,"height":126,"fontFamily":"Arial, sans-serif","fontSize":32,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":2,"borderColor":"#000000"},{"x":395,"y":312,"width":397,"height":119,"fontFamily":"Arial, sans-serif","fontSize":32,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":2,"borderColor":"#000000"}],"createdAt":"2025-08-28T10:18:07.9838353+00:00"}
    public static MemeTemplateEntity Create()
    {
        return new MemeTemplateEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003").ToString(),
            Name = "Cartoon",
            Description = "Cartoon getting in a dark version of itself",
            SourceHeight = 900,
            SourceWidth = 1600,
            CreatedAt = DateTimeOffset.UtcNow,
            SourceImageUrl = "cartoon.jpg", // Store only filename, not full URL
            PartitionKey = MemesConstants.CosmosDbPartitionKey,
            TextAreas = [
                new TextAreaEntity
                {
                    X = 10,
                    Y = 310,
                    Width = 374,
                    Height = 126,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 32,
                    FontColor = "#FFFFFF",
                    FontBold = true,
                    MaxLength = 100,
                    BorderThickness = 2,
                    BorderColor = "#000000"
                },
                new TextAreaEntity
                {
                    X = 395,
                    Y = 312,
                    Width = 397,
                    Height = 119,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 32,
                    FontColor = "#FFFFFF",
                    FontBold = true,
                    MaxLength = 100,
                    BorderThickness = 2,
                    BorderColor = "#000000"
                },
            ]
        };
    }

}

internal class AlwaysHasBeen
{
    // JSON:{"id":"03e65656-1f4b-435d-a3a1-a7af6c70cee2","name":"AlwaysHasBeen","description":"Allways has been","sourceImageUrl":"http://127.0.0.1:60375/devstoreaccount1/memes/a77080d3-fdf2-4699-8946-1c74a63358bd.webp","sourceWidth":960,"sourceHeight":540,"textAreas":[{"x":13,"y":12,"width":775,"height":66,"fontFamily":"Arial, sans-serif","fontSize":48,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":2,"borderColor":"#000000"},{"x":0,"y":365,"width":800,"height":84,"fontFamily":"Arial, sans-serif","fontSize":32,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":1,"borderColor":"#000000"}],"createdAt":"2025-08-28T10:22:28.2424765+00:00"}

    public static MemeTemplateEntity Create()
    {
        return new MemeTemplateEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004").ToString(),
            Name = "AlwaysHasBeen",
            Description = "Allways has been",
            SourceHeight = 540,
            SourceWidth = 960,
            CreatedAt = DateTimeOffset.UtcNow,
            SourceImageUrl = "always-has-been.webp", // Store only filename, not full URL
            PartitionKey = MemesConstants.CosmosDbPartitionKey,
            TextAreas = [
                new TextAreaEntity
                {
                    X = 13,
                    Y = 12,
                    Width = 775,
                    Height = 66,
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
                    X = 0,
                    Y = 365,
                    Width = 800,
                    Height = 84,
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

internal class BernieSanders
{
    // JSON: {"id":"dc14a724-ad22-477d-8c80-1c70bae0ba30","name":"bernie-sanders","sourceImageUrl":"http://127.0.0.1:54464/devstoreaccount1/memes/f255b3db-f870-43de-86ca-3384175e14c9.webp","sourceWidth":926,"sourceHeight":688,"textAreas":[{"x":0,"y":0,"width":800,"height":103,"fontFamily":"Arial, sans-serif","fontSize":48,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":2,"borderColor":"#000000"},{"x":0,"y":445,"width":800,"height":148,"fontFamily":"Arial, sans-serif","fontSize":32,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":1,"borderColor":"#000000"}],"createdAt":"2025-08-28T10:23:52.0269092+00:00"}

    public static MemeTemplateEntity Create()
    {
        return new MemeTemplateEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005").ToString(),
            Name = "bernie-sanders",
            SourceHeight = 688,
            SourceWidth = 926,
            CreatedAt = DateTimeOffset.UtcNow,
            SourceImageUrl = "bernie-sanders.webp", // Store only filename, not full URL
            PartitionKey = MemesConstants.CosmosDbPartitionKey,
            TextAreas = [
                new TextAreaEntity
                {
                    X = 0,
                    Y = 0,
                    Width = 800,
                    Height = 103,
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
                    X = 0,
                    Y = 445,
                    Width = 800,
                    Height = 148,
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

internal class ElfsTalking
{
    // JSON: {"id":"f63d07b2-3229-4093-af95-4ee0584cd15a","name":"elfs-talking","description":"Elfs having a conversation","sourceImageUrl":"http://127.0.0.1:57436/devstoreaccount1/memes/597b4201-9cf7-4a72-bdf5-f0c8478c48fa.png","sourceWidth":768,"sourceHeight":768,"textAreas":[{"x":0,"y":231,"width":297,"height":66,"fontFamily":"Arial, sans-serif","fontSize":24,"fontColor":"#FFFFFF","fontBold":false,"maxLength":100,"borderThickness":0,"borderColor":"#000000"},{"x":298,"y":230,"width":302,"height":71,"fontFamily":"Arial, sans-serif","fontSize":24,"fontColor":"#FFFFFF","fontBold":false,"maxLength":100,"borderThickness":0,"borderColor":"#000000"},{"x":0,"y":525,"width":298,"height":75,"fontFamily":"Arial, sans-serif","fontSize":24,"fontColor":"#FFFFFF","fontBold":false,"maxLength":100,"borderThickness":0,"borderColor":"#000000"},{"x":298,"y":527,"width":302,"height":73,"fontFamily":"Arial, sans-serif","fontSize":24,"fontColor":"#FFFFFF","fontBold":false,"maxLength":100,"borderThickness":0,"borderColor":"#000000"}],"createdAt":"2025-08-28T10:27:36.2820999+00:00"}
    public static MemeTemplateEntity Create()
    {
        return new MemeTemplateEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000006").ToString(),
            Name = "elfs-talking",
            Description = "Elfs having a conversation",
            SourceHeight = 768,
            SourceWidth = 768,
            CreatedAt = DateTimeOffset.UtcNow,
            SourceImageUrl = "elfs-talking.png", // Store only filename, not full URL
            PartitionKey = MemesConstants.CosmosDbPartitionKey,
            TextAreas = [
                new TextAreaEntity
                {
                    X = 0,
                    Y = 231,
                    Width = 297,
                    Height = 66,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 24,
                    FontColor = "#FFFFFF",
                    FontBold = false,
                    MaxLength = 100,
                    BorderThickness = 0,
                    BorderColor = "#000000"
                },
                new TextAreaEntity
                {
                    X = 298,
                    Y = 230,
                    Width = 302,
                    Height = 71,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 24,
                    FontColor = "#FFFFFF",
                    FontBold = false,
                    MaxLength = 100,
                    BorderThickness = 0,
                    BorderColor = "#000000"
                },
                new TextAreaEntity
                {
                    X = 0,
                    Y = 525,
                    Width = 298,
                    Height = 75,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 24,
                    FontColor = "#FFFFFF",
                    FontBold = false,
                    MaxLength = 100,
                    BorderThickness = 0,
                    BorderColor = "#000000"
                },
                new TextAreaEntity
                {
                    X = 298,
                    Y = 527,
                    Width = 302,
                    Height = 73,
                    FontFamily = "Arial, sans-serif",
                    FontSize = 24,
                    FontColor = "#FFFFFF",
                    FontBold = false,
                    MaxLength = 100,
                    BorderThickness = 0,
                    BorderColor = "#000000"
                },
            ]
        };
    }

}

internal class HandsUp
{
    // JSON: {"id":"6584eb99-a768-4d16-b970-11a8d6523df0","name":"hands-up","description":"Hands up surrender","sourceImageUrl":"http://127.0.0.1:57436/devstoreaccount1/memes/4f843506-e06b-470b-8784-d47401005578.webp","sourceWidth":936,"sourceHeight":725,"textAreas":[{"x":0,"y":0,"width":769,"height":121,"fontFamily":"Arial, sans-serif","fontSize":48,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":2,"borderColor":"#000000"},{"x":0,"y":504,"width":775,"height":95,"fontFamily":"Arial, sans-serif","fontSize":32,"fontColor":"#FFFFFF","fontBold":true,"maxLength":100,"borderThickness":1,"borderColor":"#000000"}],"createdAt":"2025-08-28T10:28:57.0242782+00:00"}

    public static MemeTemplateEntity Create()
    {
        return new MemeTemplateEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000007").ToString(),
            Name = "hands-up",
            Description = "Hands up surrender",
            SourceHeight = 725,
            SourceWidth = 936,
            CreatedAt = DateTimeOffset.UtcNow,
            SourceImageUrl = "hands-up.webp", // Store only filename, not full URL
            PartitionKey = MemesConstants.CosmosDbPartitionKey,
            TextAreas = [
                new TextAreaEntity
                {
                    X = 0,
                    Y = 0,
                    Width = 769,
                    Height = 121,
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
                    X = 0,
                    Y = 504,
                    Width = 775,
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