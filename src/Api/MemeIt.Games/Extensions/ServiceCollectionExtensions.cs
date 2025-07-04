using Microsoft.Extensions.DependencyInjection;
using MemeIt.Games.Services;
using MemeIt.Games.Abstractions.Services;

namespace MemeIt.Games.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.AddScoped<IGameService, GameService>();
        
        return services;
    }
}
