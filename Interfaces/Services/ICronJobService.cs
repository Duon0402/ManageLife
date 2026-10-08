using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ICronJobService
    {
        Task<Result<List<CronJobViewModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result<CronJobViewModel>> GetByIdAsync(int jobId, CancellationToken ct = default);
        Task<Result> CreateAsync(SaveCronJobRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(SaveCronJobRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(CronJobIdRequest request, CancellationToken ct = default);
        Task<Result<List<CronJobHistoryModel>>> GetHistoryAsync(int jobId, CancellationToken ct = default);

        /// <summary>Tạo/cập nhật các job hệ thống khai báo trong code (khớp theo URL, chạy lại nhiều lần không sinh trùng).</summary>
        Task<Result<string>> SyncSystemJobsAsync(CancellationToken ct = default);
    }
}
