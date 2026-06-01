using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Repositories
{
    public class CodeSequenceRepository : RepositoryBase<CodeSequenceEntity>, ICodeSequenceRepository
    {
        public CodeSequenceRepository(IUnitOfWork uow, IUserContext userContext)
            : base(uow, userContext) { }

        public async Task<CodeSequenceEntity> IncrementAndGetAsync(string category, CancellationToken ct = default)
        {
            await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var entity = await _context.Set<CodeSequenceEntity>()
                    .FromSqlRaw("SELECT * FROM `CodeSequences` WHERE `Category` = {0} FOR UPDATE", category)
                    .FirstOrDefaultAsync(ct)
                    ?? throw new InvalidOperationException($"CodeSequence '{category}' chưa được khởi tạo.");

                entity.CurrentSeq++;
                await _context.SaveChangesAsync(ct);
                await _context.Database.CommitTransactionAsync(ct);
                return entity;
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}
