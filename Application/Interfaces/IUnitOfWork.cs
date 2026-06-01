namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        void ClearChanges();

        Task SaveChangesAsync();

        Task ExecuteInTransactionAsync(Func<Task> action);

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
    }
}
