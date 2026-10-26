using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IFinanceService
    {
        /// <summary>Tạo ví "Tiền mặt" + danh mục mặc định nếu người dùng chưa có danh mục nào (lần đồng bộ đầu).</summary>
        Task<Result> EnsureDefaultDataAsync(CancellationToken ct = default);

        Task<Result> SaveWalletAsync(string id, SaveFinanceWalletRequest request, CancellationToken ct = default);
        Task<Result> DeleteWalletAsync(string id, CancellationToken ct = default);
        Task<Result> ReorderWalletsAsync(ReorderRequest request, CancellationToken ct = default);

        Task<Result> SaveCategoryAsync(string id, SaveFinanceCategoryRequest request, CancellationToken ct = default);
        Task<Result> DeleteCategoryAsync(string id, CancellationToken ct = default);
        Task<Result> ReorderCategoriesAsync(ReorderRequest request, CancellationToken ct = default);

        Task<Result> SaveTransactionAsync(string id, SaveFinanceTransactionRequest request, CancellationToken ct = default);
        Task<Result> DeleteTransactionAsync(string id, CancellationToken ct = default);

        Task<Result> SaveBudgetAsync(string id, SaveFinanceBudgetRequest request, CancellationToken ct = default);
        Task<Result> DeleteBudgetAsync(string id, CancellationToken ct = default);

        Task<Result> SaveRecurringAsync(string id, SaveFinanceRecurringRequest request, CancellationToken ct = default);
        Task<Result> DeleteRecurringAsync(string id, CancellationToken ct = default);

        Task<Result> SaveDebtAsync(string id, SaveFinanceDebtRequest request, CancellationToken ct = default);
        Task<Result> DeleteDebtAsync(string id, CancellationToken ct = default);
    }
}
