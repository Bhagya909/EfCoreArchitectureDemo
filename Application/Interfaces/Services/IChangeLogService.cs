using Application.Common;
using Application.DTOs.Logging;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface IChangeLogService
    {
        Task LogAsync(
            string actionType,
            string entityName,
            int? referenceId,
            string description,
            string? rawData = null,
            bool requestAiSummary = false,
            Guid? correlationId = null);
        Task<ChangeLogDetailResponseDto?> GetByIdAsync(int id);
        Task<PagedResult<ChangeLogResponseDto>> GetPagedAsync(
            ChangeLogQueryParameters parameters);
        Task<PagedResult<ChangeLogResponseDto>> GetPendingAiAsync(
            int page, int pageSize);
        Task<PagedResult<ChangeLogResponseDto>> GetAiCompletedAsync(
            int page, int pageSize);
        Task<ChangeLogSummaryDto> GetSummaryAsync();
        Task<int> BulkDeleteOldLogsAsync(BulkDeleteOldLogsDto dto);
        Task<int> BulkDeleteLogsByActionTypeAsync(
            BulkDeleteLogsByActionTypeDto dto);
    }
}