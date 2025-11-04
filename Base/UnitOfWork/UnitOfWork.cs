using ManageLife.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ManageLife.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly IExecutionStrategy _strategy;
        private bool _committed;
        private bool _disposed;

        public UnitOfWork(AppDbContext context, bool autoStartTransaction = true)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _strategy = _context.Database.CreateExecutionStrategy();

            if (autoStartTransaction)
                BeginTransactionAsync().GetAwaiter().GetResult();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            await _strategy.ExecuteAsync(async () =>
            {
                _transaction ??= await _context.Database.BeginTransactionAsync();
            });
        }

        public async Task CommitAsync()
        {
            EnsureNotDisposed();

            await _context.SaveChangesAsync();

            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                _committed = true;
            }
        }

        public async Task RollbackAsync()
        {
            EnsureNotDisposed();
            await SafeRollbackAsync();
        }

        private async Task SafeRollbackAsync()
        {
            if (_transaction != null && !_committed)
            {
                try { await _transaction.RollbackAsync(); } catch { }
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            await SafeRollbackAsync();

            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            _disposed = true;
        }

        public void Dispose() =>
            DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
