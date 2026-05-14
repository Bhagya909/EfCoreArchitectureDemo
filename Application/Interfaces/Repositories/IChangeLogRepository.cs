using Domain.Entities.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IChangeLogRepository
    {
        // change log repository interface
        Task AddAsync(ChangeLog changeLog);

    }
}
