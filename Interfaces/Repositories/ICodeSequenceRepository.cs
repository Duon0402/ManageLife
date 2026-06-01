using ManageLife.Core;
using ManageLife.Entities;

namespace ManageLife.Interfaces
{
    public interface ICodeSequenceRepository : IRepositoryBase<CodeSequenceEntity>
    {
        Task<CodeSequenceEntity> IncrementAndGetAsync(string category, CancellationToken ct = default);
    }
}
