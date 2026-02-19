using ManageLife.Data;

namespace ManageLife.Base
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        AppDbContext Context { get; }
        bool AutoSave { get; }
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync();
    }
}
