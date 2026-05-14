using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Logging;
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
    string? rawData,
    string? summary)
        {
            var log = new ChangeLog(
                actionType,
                entityName,
                referenceId,
                rawData,
                summary);

            await _changeLogRepository
                .AddAsync(log);
        }
    }
}
