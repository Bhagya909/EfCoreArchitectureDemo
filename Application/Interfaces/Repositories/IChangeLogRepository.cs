using Application.Common;
using Application.DTOs.Logging;
using Application.Models;
using Domain.Entities.Logging;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IChangeLogRepository
    {
        Task AddAsync(ChangeLog changeLog);
        Task<ChangeLogDetailResponseDto?> GetByIdAsync(int id);
        Task<PagedResult<ChangeLogResponseDto>> GetPagedAsync(
            ChangeLogQueryParameters parameters);
        Task<PagedResult<ChangeLogResponseDto>> GetPendingAiAsync(
            int page, int pageSize);
        Task<PagedResult<ChangeLogResponseDto>> GetAiCompletedAsync(
            int page, int pageSize);
        Task<ChangeLogSummaryDto> GetSummaryAsync();
        Task<int> BulkDeleteOldLogsAsync(DateTime cutoffDate);
        Task<int> BulkDeleteLogsByActionTypeAsync(
            string actionType, DateTime cutoffDate);
        Task<List<ChangeLog>> GetPendingAiSummariesAsync(
            int batchSize);
        Task SaveChangesAsync();
    }
}