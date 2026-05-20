using Application.DTOs.Logging;
using System;
using System.Collections.Generic;
using System.Text;

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
        Task<int> BulkDeleteOldLogsAsync(
    BulkDeleteOldLogsDto dto);

        Task<int> BulkDeleteLogsByActionTypeAsync(
            BulkDeleteLogsByActionTypeDto dto);
    }
}
