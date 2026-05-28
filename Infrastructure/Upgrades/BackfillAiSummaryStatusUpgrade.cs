using Application.Interfaces.Upgrades;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Upgrades
{
    public class BackfillAiSummaryStatusUpgrade : IDataUpgrade
    {
        private readonly RetailDbContext _context;

        public string Name =>
            "BackfillAiSummaryStatus";

        public BackfillAiSummaryStatusUpgrade(
            RetailDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            var logs = await _context.ChangeLogs
                .Where(log =>
                    log.AiSummaryStatus == AiSummaryStatus.NotRequested &&
                    (
                        log.AiSummary != null ||
                        log.AiSummaryError != null ||
                        (log.LogSource == LogSource.Audit &&
                            log.ChangedFields != null)
                    ))
                .ToListAsync();

            foreach (var log in logs)
            {
                if (!string.IsNullOrWhiteSpace(log.AiSummary))
                {
                    log.CompleteAiSummary(log.AiSummary);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(log.AiSummaryError))
                {
                    log.FailAiSummary(log.AiSummaryError);
                    continue;
                }

                log.SkipAiSummary();
            }
        }
    }
}
