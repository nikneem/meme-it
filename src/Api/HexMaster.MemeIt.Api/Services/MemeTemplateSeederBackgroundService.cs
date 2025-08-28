using HexMaster.MemeIt.Aspire;
using HexMaster.MemeIt.Memes.Services;
using Microsoft.Azure.Cosmos;

namespace HexMaster.MemeIt.Api.Services;

public class MemeTemplateSeederBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MemeTemplateSeederBackgroundService> _logger;
    private const int StartupDelayMilliseconds = 5000; // 5 seconds

    public MemeTemplateSeederBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MemeTemplateSeederBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Wait for startup delay
            _logger.LogInformation("MemeTemplateSeederBackgroundService starting with {Delay}ms delay...", StartupDelayMilliseconds);
            await Task.Delay(StartupDelayMilliseconds, stoppingToken);

            
if (stoppingToken.IsCancellationRequested)
                return;

            using var scope = _serviceProvider.CreateScope();

            var cosmosContainer = scope.ServiceProvider.GetRequiredService<Container>();
            var memeTemplateSeeder = scope.ServiceProvider.GetRequiredService<MemeTemplateSeeder>();

            await memeTemplateSeeder.SeedTemplatesAsync(
                cosmosContainer,
                stoppingToken);

            _logger.LogInformation("MemeTemplateSeederBackgroundService completed successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MemeTemplateSeederBackgroundService was cancelled during startup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during meme template seeding");
        }

        // Background service completes after one execution
    }
}