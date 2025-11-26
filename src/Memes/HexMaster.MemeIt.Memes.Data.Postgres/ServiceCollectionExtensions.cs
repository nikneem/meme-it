using HexMaster.MemeIt.Memes.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.MemeIt.Memes.Data.Postgres;

/// <summary>
/// Dependency injection helpers for PostgreSQL-based persistence.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMemesPostgresData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<MemesDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("memes-db");
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IMemeTemplateRepository, PostgresMemeTemplateRepository>();

        return services;
    }
}
