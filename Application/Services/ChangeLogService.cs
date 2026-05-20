using Application.DTOs.Logging;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Logging;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
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
            {
                log.MarkAiSummaryPending();
            }
            else
            {
                log.SkipAiSummary();
            }

            await _changeLogRepository
                .AddAsync(log);
        }
        public async Task<int> BulkDeleteOldLogsAsync(
       BulkDeleteOldLogsDto dto)
        {
            if (dto.OlderThanDays < 7)
            {
                throw new Exception(
                    "Retention period must be at least 7 days.");
            }

            var cutoffDate =
                DateTime.UtcNow.AddDays(
                    -dto.OlderThanDays);

            var deletedRows =
                await _changeLogRepository
                    .BulkDeleteOldLogsAsync(
                        cutoffDate);

            return deletedRows;
        }
        public async Task<int> BulkDeleteLogsByActionTypeAsync(
    BulkDeleteLogsByActionTypeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ActionType))
            {
                throw new Exception(
                    "Action type is required.");
            }

            if (dto.OlderThanDays < 7)
            {
                throw new Exception(
                    "Retention period must be at least 7 days.");
            }

            var protectedActionTypes =
                new[]
                {
            "PAYMENT_COMPLETED",
            "ORDER_CREATED"
                };

            if (protectedActionTypes.Contains(
                dto.ActionType,
                StringComparer.OrdinalIgnoreCase))
            {
                throw new Exception(
                    "Critical audit logs cannot be bulk deleted.");
            }

            var cutoffDate =
                DateTime.UtcNow.AddDays(
                    -dto.OlderThanDays);

            var deletedRows =
                await _changeLogRepository
                    .BulkDeleteLogsByActionTypeAsync(
                        dto.ActionType,
                        cutoffDate);

            return deletedRows;
        }
    }
}
