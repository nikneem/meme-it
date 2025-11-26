using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HexMaster.MemeIt.Memes.Data.Postgres;

/// <summary>
/// Design-time factory for creating DbContext instances during migrations.
/// </summary>
public class MemesDbContextFactory : IDesignTimeDbContextFactory<MemesDbContext>
{
    public MemesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MemesDbContext>();

        // Use a default connection string for migrations
        // This will be overridden at runtime by the actual configuration
        optionsBuilder.UseNpgsql("Host=localhost;Database=memesdb;Username=postgres;Password=postgres");

        return new MemesDbContext(optionsBuilder.Options);
    }
}
