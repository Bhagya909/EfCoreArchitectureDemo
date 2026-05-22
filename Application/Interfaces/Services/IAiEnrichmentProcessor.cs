namespace Application.Interfaces.Services;

public interface IAiEnrichmentProcessor
{
    Task ProcessPendingSummariesAsync(
        int batchSize = 10);
}