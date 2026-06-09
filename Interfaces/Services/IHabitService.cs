using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IHabitService
    {
        Task<Result<List<HabitModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result> CreateAsync(CreateHabitRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateHabitRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(DeleteHabitRequest request, CancellationToken ct = default);
    }
}
