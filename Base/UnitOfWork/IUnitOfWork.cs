namespace ManageLife.Base
{
    public interface IUnitOfWork : IDisposable
    {
        bool AutoProcess { get; }
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
