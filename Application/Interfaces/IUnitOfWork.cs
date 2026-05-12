using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        Task SaveChangesAsync();
    }
}
