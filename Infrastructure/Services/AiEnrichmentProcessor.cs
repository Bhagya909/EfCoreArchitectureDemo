using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class AiEnrichmentProcessor
    : IAiEnrichmentProcessor
{
    private readonly IChangeLogRepository
        _changeLogRepository;

    private readonly IAiSummaryService
        _aiSummaryService;

    public AiEnrichmentProcessor(
        IChangeLogRepository changeLogRepository,
        IAiSummaryService aiSummaryService)
    {
        _changeLogRepository =
            changeLogRepository;

        _aiSummaryService =
            aiSummaryService;
    }

    public async Task ProcessPendingSummariesAsync(
        int batchSize = 10)
    {
        var pendingLogs =
            await _changeLogRepository
                .GetPendingAiSummariesAsync(
                    batchSize);

        foreach (var log in pendingLogs)
        {
            try
            {
                var summary =
                    await _aiSummaryService
                        .GenerateSummaryAsync(log);

                log.CompleteAiSummary(summary);
            }
            catch (Exception ex)
            {
                log.FailAiSummary(ex.Message);
            }
        }

        await _changeLogRepository
            .SaveChangesAsync();
    }
}