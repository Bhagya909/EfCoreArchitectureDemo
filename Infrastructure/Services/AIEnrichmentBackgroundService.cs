using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AiEnrichmentBackgroundService
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AiEnrichmentBackgroundService> _logger;

    public AiEnrichmentBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AiEnrichmentBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _serviceProvider.CreateScope();

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<IAiEnrichmentProcessor>();

                await processor
                    .ProcessPendingSummariesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "AI enrichment cycle failed: {Message}",
                    ex.Message);
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}