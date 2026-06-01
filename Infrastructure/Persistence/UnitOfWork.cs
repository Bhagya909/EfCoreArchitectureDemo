using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RetailDbContext _context;

        private IDbContextTransaction? _transaction;

        public UnitOfWork(RetailDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction =
                await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }

        public void ClearChanges()
        {
            _context.ChangeTracker.Clear();
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyConflictException(
                    "A concurrency conflict occurred. The record was modified by another request.",
                    ex);
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await BeginTransactionAsync();

            try
            {
                await action();
                await CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action)
        {
            await BeginTransactionAsync();

            try
            {
                var result = await action();
                await CommitTransactionAsync();

                return result;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }
    }
}
