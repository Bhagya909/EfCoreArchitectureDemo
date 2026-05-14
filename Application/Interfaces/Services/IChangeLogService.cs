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
        string? rawData,
        string? summary);
    }
}
