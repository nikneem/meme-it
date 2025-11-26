using HexMaster.MemeIt.Memes.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HexMaster.MemeIt.Memes.Data.Postgres.Configurations;

/// <summary>
/// Entity Framework configuration for MemeTemplate aggregate.
/// </summary>
public class MemeTemplateConfiguration : IEntityTypeConfiguration<MemeTemplate>
{
    public void Configure(EntityTypeBuilder<MemeTemplate> builder)
    {
        builder.ToTable("MemeTemplates");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.ImageUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Width)
            .IsRequired();

        builder.Property(m => m.Height)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        // Configure TextAreas as owned entities (value objects)
        builder.OwnsMany(m => m.TextAreas, textArea =>
        {
            textArea.ToTable("MemeTemplateTextAreas");

            textArea.WithOwner()
                .HasForeignKey("MemeTemplateId");

            textArea.Property<int>("Id")
                .ValueGeneratedOnAdd();

            textArea.HasKey("Id");

            textArea.Property(t => t.X)
                .IsRequired();

            textArea.Property(t => t.Y)
                .IsRequired();

            textArea.Property(t => t.Width)
                .IsRequired();

            textArea.Property(t => t.Height)
                .IsRequired();

            textArea.Property(t => t.FontSize)
                .IsRequired();

            textArea.Property(t => t.FontColor)
                .IsRequired()
                .HasMaxLength(7);

            textArea.Property(t => t.BorderSize)
                .IsRequired();

            textArea.Property(t => t.BorderColor)
                .IsRequired()
                .HasMaxLength(7);

            textArea.Property(t => t.IsBold)
                .IsRequired();
        });

        builder.Navigation(m => m.TextAreas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
