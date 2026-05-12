using Domain.Entities.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IChangeLogRepository
    {
        Task AddAsync(ChangeLog changeLog);

    }
}
