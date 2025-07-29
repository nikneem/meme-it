using HexMaster.MemeIt.Memes.Models.Entities;

namespace HexMaster.MemeIt.Memes.Models.Mappers;

/// <summary>
/// Maps between domain models and CosmosDB entities
/// </summary>
public static class MemeTemplateMapper
{
    /// <summary>
    /// Maps a domain model to a CosmosDB entity
    /// </summary>
    public static MemeTemplateEntity ToEntity(MemeTemplate memeTemplate)
    {
        if (memeTemplate == null)
            throw new ArgumentNullException(nameof(memeTemplate));

        return new MemeTemplateEntity
        {
            Id = memeTemplate.Id,
            Name = memeTemplate.Name,
            Description = memeTemplate.Description,
            SourceImageUrl = memeTemplate.SourceImageUrl,
            SourceWidth = memeTemplate.SourceWidth,
            SourceHeight = memeTemplate.SourceHeight,
            TextAreas = memeTemplate.TextAreas.Select(ToEntity).ToArray(),
            CreatedAt = memeTemplate.CreatedAt,
            UpdatedAt = memeTemplate.UpdatedAt,
            ETag = memeTemplate.ETag,
            PartitionKey = MemesConstants.CosmosDbPartitionKey
        };
    }

    /// <summary>
    /// Maps a CosmosDB entity to a domain model
    /// </summary>
    public static MemeTemplate ToDomainModel(MemeTemplateEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var textAreas = entity.TextAreas?.Select(ToDomainModel) ?? Enumerable.Empty<TextArea>();
        
        var memeTemplate = new MemeTemplate(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.SourceImageUrl,
            entity.SourceWidth,
            entity.SourceHeight,
            textAreas,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.ETag);

        return memeTemplate;
    }

    /// <summary>
    /// Maps a domain model text area to a CosmosDB entity
    /// </summary>
    private static TextAreaEntity ToEntity(TextArea textArea)
    {
        return new TextAreaEntity
        {
            X = textArea.X,
            Y = textArea.Y,
            Width = textArea.Width,
            Height = textArea.Height,
            FontFamily = textArea.FontFamily,
            FontSize = textArea.FontSize,
            FontColor = textArea.FontColor,
            FontBold = textArea.FontBold,
            MaxLength = textArea.MaxLength,
            BorderThickness = textArea.BorderThickness,
            BorderColor = textArea.BorderColor
        };
    }

    /// <summary>
    /// Maps a CosmosDB entity text area to a domain model
    /// </summary>
    private static TextArea ToDomainModel(TextAreaEntity entity)
    {
        return new TextArea(
            entity.X,
            entity.Y,
            entity.Width,
            entity.Height,
            entity.FontFamily,
            entity.FontSize,
            entity.FontColor,
            entity.FontBold,
            entity.MaxLength,
            entity.BorderThickness,
            entity.BorderColor);
    }
}
