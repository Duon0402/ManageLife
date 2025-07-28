using ManageLife.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ManageLife.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        public bool AutoProcess { get; }

        public UnitOfWork(AppDbContext context, bool autoProcess = true)
        {
            _context = context;
            AutoProcess = autoProcess;

            if (AutoProcess)
            {
                BeginTransactionAsync().GetAwaiter().GetResult();
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();

            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
