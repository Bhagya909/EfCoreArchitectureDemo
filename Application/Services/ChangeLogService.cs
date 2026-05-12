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
        private readonly IChangeLogRepository _repository;

        public ChangeLogService(
            IChangeLogRepository repository)
        {
            _repository = repository;
        }

        public async Task LogAsync(
            string actionType,
            string entityName,
            int? referenceId,
            string rawData,
            string? summary = null)
        {
            var log = new ChangeLog(
                actionType,
                entityName,
                referenceId,
                rawData,
                summary);

            await _repository.AddAsync(log);
        }
    }
}
