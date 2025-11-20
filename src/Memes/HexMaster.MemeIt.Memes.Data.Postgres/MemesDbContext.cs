using HexMaster.MemeIt.Memes.Domains;
using Microsoft.EntityFrameworkCore;

namespace HexMaster.MemeIt.Memes.Data.Postgres;

/// <summary>
/// Entity Framework Core DbContext for the Memes module.
/// </summary>
public class MemesDbContext : DbContext
{
    public MemesDbContext(DbContextOptions<MemesDbContext> options) : base(options)
    {
    }

    public DbSet<MemeTemplate> MemeTemplates => Set<MemeTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MemesDbContext).Assembly);
    }
}
