using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Services;

namespace HexMaster.MemeIt.Games.Api.Infrastructure;

public static class SchedulingServiceCollectionExtensions
{
    public static IServiceCollection AddScheduledTaskService(this IServiceCollection services)
    {
        services.AddSingleton<ScheduledTaskService>();
        services.AddSingleton<IScheduledTaskService>(sp => sp.GetRequiredService<ScheduledTaskService>());
        services.AddHostedService<ScheduledTaskWorker>();
        return services;
    }
}
