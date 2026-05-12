using Application.Interfaces.Repositories;
using Domain.Entities.Logging;
using Infrastructure.Persistence;
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

     
    }
}
