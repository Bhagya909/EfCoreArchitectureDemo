using Domain.Entities.Logging;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IChangeLogRepository
    {
        // change log repository interface
        Task AddAsync(ChangeLog changeLog);
        Task<int> BulkDeleteOldLogsAsync(
    DateTime cutoffDate);

        Task<int> BulkDeleteLogsByActionTypeAsync(
            string actionType,
            DateTime cutoffDate);
        Task<List<ChangeLog>> GetPendingAiSummariesAsync(
            int batchSize);

        Task SaveChangesAsync();
    }
}
