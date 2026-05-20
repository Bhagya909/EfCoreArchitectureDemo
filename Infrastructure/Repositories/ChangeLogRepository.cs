using Application.Interfaces.Repositories;
using Domain.Entities.Logging;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
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
        public async Task<int> BulkDeleteOldLogsAsync(
    DateTime cutoffDate)
        {
            return await _context.ChangeLogs
                .Where(log =>
                    log.CreatedAt < cutoffDate)
                .ExecuteDeleteAsync();
        }
        public async Task<int> BulkDeleteLogsByActionTypeAsync(
    string actionType,
    DateTime cutoffDate)
        {
            return await _context.ChangeLogs
                .Where(log =>
                    log.ActionType == actionType &&
                    log.CreatedAt < cutoffDate)
                .ExecuteDeleteAsync();
        }
        public async Task<List<ChangeLog>>
            GetPendingAiSummariesAsync(
                int batchSize)
        {
            return await _context.ChangeLogs
                .Where(log =>
                    log.AiSummaryStatus ==
                    AiSummaryStatus.Pending)
                .OrderBy(log => log.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
