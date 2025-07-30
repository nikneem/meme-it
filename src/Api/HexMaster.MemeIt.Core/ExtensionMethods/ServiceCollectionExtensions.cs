using HexMaster.MemeIt.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.MemeIt.Core.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebPubSubServices(this IServiceCollection services)
    {
        services.AddSingleton<IWebPubSubService, WebPubSubService>();
        return services;
    }
}
