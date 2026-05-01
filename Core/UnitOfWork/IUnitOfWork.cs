using ManageLife.Data;

namespace ManageLife.Core
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        AppDbContext Context { get; }
        bool AutoSave { get; }
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
