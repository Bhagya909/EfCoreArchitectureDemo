using Application.Common;
using Application.DTOs.Logging;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities.Logging;
using Domain.Enums;

namespace Application.Services.Logging
{
    public class ChangeLogService : IChangeLogService
    {
        private readonly IChangeLogRepository _changeLogRepository;

        public ChangeLogService(
            IChangeLogRepository repository)
        {
            _changeLogRepository = repository;
        }

        public async Task LogAsync(
            string actionType,
            string entityName,
            int? referenceId,
            string description,
            string? rawData = null,
            bool requestAiSummary = false,
            Guid? correlationId = null)
        {
            var log = new ChangeLog(
                actionType: actionType,
                entityName: entityName,
                referenceId: referenceId,
                rawData: rawData ?? string.Empty,
                description: description,
                logSource: LogSource.Business,
                correlationId:
                    correlationId ?? Guid.NewGuid());

            log.SetCategory("BUSINESS");
            log.SetSeverity("INFO");

            if (requestAiSummary)
                log.MarkAiSummaryPending();
            else
                log.SkipAiSummary();

            await _changeLogRepository.AddAsync(log);
        }

        public async Task<ChangeLogDetailResponseDto?> GetByIdAsync(
            int id)
        {
            return await _changeLogRepository.GetByIdAsync(id);
        }

        public async Task<PagedResult<ChangeLogResponseDto>> GetPagedAsync(
            ChangeLogQueryParameters parameters)
        {
            if (parameters.Page < 1)
                parameters.Page = 1;

            if (parameters.PageSize < 1 || parameters.PageSize > 100)
                parameters.PageSize = 20;

            if (parameters.FromDate.HasValue
                && parameters.ToDate.HasValue
                && parameters.FromDate > parameters.ToDate)
                throw new ArgumentException(
                    "FromDate cannot be greater than ToDate.");

            return await _changeLogRepository
                .GetPagedAsync(parameters);
        }

        public async Task<PagedResult<ChangeLogResponseDto>> GetPendingAiAsync(
            int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return await _changeLogRepository
                .GetPendingAiAsync(page, pageSize);
        }

        public async Task<PagedResult<ChangeLogResponseDto>> GetAiCompletedAsync(
            int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            return await _changeLogRepository
                .GetAiCompletedAsync(page, pageSize);
        }

        public async Task<ChangeLogSummaryDto> GetSummaryAsync()
        {
            return await _changeLogRepository.GetSummaryAsync();
        }

        public async Task<int> BulkDeleteOldLogsAsync(
            BulkDeleteOldLogsDto dto)
        {
            if (dto.OlderThanDays < 7)
                throw new ArgumentException(
                    "Retention period must be at least 7 days.");

            var cutoffDate = DateTime.UtcNow
                .AddDays(-dto.OlderThanDays);

            return await _changeLogRepository
                .BulkDeleteOldLogsAsync(cutoffDate);
        }

        public async Task<int> BulkDeleteLogsByActionTypeAsync(
            BulkDeleteLogsByActionTypeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ActionType))
                throw new ArgumentException(
                    "Action type is required.");

            if (dto.OlderThanDays < 7)
                throw new ArgumentException(
                    "Retention period must be at least 7 days.");

            var protectedActionTypes = new[]
            {
                "PAYMENT_COMPLETED",
                "ORDER_CREATED"
            };

            if (protectedActionTypes.Contains(
                dto.ActionType,
                StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Action type '{dto.ActionType}' is protected " +
                    $"and cannot be bulk deleted.");

            var cutoffDate = DateTime.UtcNow
                .AddDays(-dto.OlderThanDays);

            return await _changeLogRepository
                .BulkDeleteLogsByActionTypeAsync(
                    dto.ActionType,
                    cutoffDate);
        }
    }
}