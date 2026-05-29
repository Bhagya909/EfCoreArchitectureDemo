using Application.Common;
using Application.DTOs.Logging;
using Application.Interfaces.Repositories;
using Application.Models;
using Domain.Entities.Logging;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Logging
{
    public class ChangeLogRepository : IChangeLogRepository
    {
        private readonly RetailDbContext _context;

        public ChangeLogRepository(RetailDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ChangeLog changeLog)
        {
            await _context.ChangeLogs.AddAsync(changeLog);
        }

        public async Task<ChangeLogDetailResponseDto?> GetByIdAsync(
            int id)
        {
            return await _context.ChangeLogs
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new ChangeLogDetailResponseDto
                {
                    Id = l.Id,
                    ActionType = l.ActionType,
                    EntityName = l.EntityName,
                    ReferenceId = l.ReferenceId,
                    Description = l.Description,
                    RawData = l.RawData,
                    LogSource = l.LogSource.ToString(),
                    CorrelationId = l.CorrelationId,
                    Category = l.Category,
                    Severity = l.Severity,
                    ChangedFields = l.ChangedFields,
                    OldValues = l.OldValues,
                    NewValues = l.NewValues,
                    AiSummaryStatus = l.AiSummaryStatus.ToString(),
                    AiSummary = l.AiSummary,
                    AiSummaryError = l.AiSummaryError,
                    AiSummaryGeneratedAt = l.AiSummaryGeneratedAt,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<ChangeLogResponseDto>> GetPagedAsync(
            ChangeLogQueryParameters parameters)
        {
            var query = _context.ChangeLogs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.EntityName))
                query = query.Where(l =>
                    l.EntityName == parameters.EntityName);

            if (!string.IsNullOrWhiteSpace(parameters.ActionType))
                query = query.Where(l =>
                    l.ActionType == parameters.ActionType);

            if (!string.IsNullOrWhiteSpace(parameters.LogSource)
                && Enum.TryParse<LogSource>(
                    parameters.LogSource,
                    ignoreCase: true,
                    out var parsedSource))
                query = query.Where(l =>
                    l.LogSource == parsedSource);

            if (parameters.FromDate.HasValue)
                query = query.Where(l =>
                    l.CreatedAt >= parameters.FromDate.Value);

            if (parameters.ToDate.HasValue)
                query = query.Where(l =>
                    l.CreatedAt <= parameters.ToDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(l => new ChangeLogResponseDto
                {
                    Id = l.Id,
                    ActionType = l.ActionType,
                    EntityName = l.EntityName,
                    ReferenceId = l.ReferenceId,
                    Description = l.Description,
                    LogSource = l.LogSource.ToString(),
                    Category = l.Category,
                    Severity = l.Severity,
                    AiSummaryStatus = l.AiSummaryStatus.ToString(),
                    AiSummary = l.AiSummary,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<ChangeLogResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<PagedResult<ChangeLogResponseDto>> GetPendingAiAsync(
            int page, int pageSize)
        {
            var query = _context.ChangeLogs
                .AsNoTracking()
                .Where(l => l.AiSummaryStatus == AiSummaryStatus.Pending);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new ChangeLogResponseDto
                {
                    Id = l.Id,
                    ActionType = l.ActionType,
                    EntityName = l.EntityName,
                    ReferenceId = l.ReferenceId,
                    Description = l.Description,
                    LogSource = l.LogSource.ToString(),
                    Category = l.Category,
                    Severity = l.Severity,
                    AiSummaryStatus = l.AiSummaryStatus.ToString(),
                    AiSummary = l.AiSummary,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<ChangeLogResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ChangeLogResponseDto>> GetAiCompletedAsync(
            int page, int pageSize)
        {
            var query = _context.ChangeLogs
                .AsNoTracking()
                .Where(l => l.AiSummaryStatus == AiSummaryStatus.Completed);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.AiSummaryGeneratedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new ChangeLogResponseDto
                {
                    Id = l.Id,
                    ActionType = l.ActionType,
                    EntityName = l.EntityName,
                    ReferenceId = l.ReferenceId,
                    Description = l.Description,
                    LogSource = l.LogSource.ToString(),
                    Category = l.Category,
                    Severity = l.Severity,
                    AiSummaryStatus = l.AiSummaryStatus.ToString(),
                    AiSummary = l.AiSummary,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<ChangeLogResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ChangeLogSummaryDto> GetSummaryAsync()
        {
            var query = _context.ChangeLogs.AsNoTracking();

            var total = await query.CountAsync();

            var businessLogs = await query
                .CountAsync(l => l.LogSource == LogSource.Business);

            var auditLogs = await query
                .CountAsync(l => l.LogSource == LogSource.Audit);

            var pendingAi = await query
                .CountAsync(l => l.AiSummaryStatus == AiSummaryStatus.Pending);

            var completedAi = await query
                .CountAsync(l => l.AiSummaryStatus == AiSummaryStatus.Completed);

            var failedAi = await query
                .CountAsync(l => l.AiSummaryStatus == AiSummaryStatus.Failed);

            var skippedAi = await query
                .CountAsync(l => l.AiSummaryStatus == AiSummaryStatus.Skipped);

            var notRequestedAi = await query
                .CountAsync(l => l.AiSummaryStatus == AiSummaryStatus.NotRequested);

            return new ChangeLogSummaryDto
            {
                TotalLogs = total,
                BusinessLogs = businessLogs,
                AuditLogs = auditLogs,
                PendingAiCount = pendingAi,
                CompletedAiCount = completedAi,
                FailedAiCount = failedAi,
                SkippedAiCount = skippedAi,
                NotRequestedAiCount = notRequestedAi
            };
        }

        public async Task<int> BulkDeleteOldLogsAsync(
            DateTime cutoffDate)
        {
            return await _context.ChangeLogs
                .Where(l => l.CreatedAt < cutoffDate)
                .ExecuteDeleteAsync();
        }

        public async Task<int> BulkDeleteLogsByActionTypeAsync(
            string actionType,
            DateTime cutoffDate)
        {
            return await _context.ChangeLogs
                .Where(l =>
                    l.ActionType == actionType &&
                    l.CreatedAt < cutoffDate)
                .ExecuteDeleteAsync();
        }

        public async Task<List<ChangeLog>> GetPendingAiSummariesAsync(
            int batchSize)
        {
            return await _context.ChangeLogs
                .Where(l => l.AiSummaryStatus == AiSummaryStatus.Pending)
                .OrderBy(l => l.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}