using Domain.Entities.Logging;

namespace Application.Interfaces.Services;

public interface IAiSummaryService
{
    Task<string> GenerateSummaryAsync(
        ChangeLog changeLog);
}