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

        public bool AutoProcess { get; }


        public UnitOfWork(AppDbContext context, bool autoProcess = true)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _strategy = _context.Database.CreateExecutionStrategy();

            if (autoProcess)
            {
                _strategy.Execute(() =>
                {
                    _transaction = _context.Database.BeginTransaction();
                    return true;
                });
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                await _strategy.ExecuteAsync(async () =>
                {
                    _transaction = await _context.Database.BeginTransactionAsync();
                });
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
