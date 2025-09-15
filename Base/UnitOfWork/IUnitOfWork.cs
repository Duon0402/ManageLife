namespace ManageLife.Base
{
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
