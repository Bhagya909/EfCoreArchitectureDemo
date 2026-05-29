namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        void ClearChanges();

        Task SaveChangesAsync();
    }
}
