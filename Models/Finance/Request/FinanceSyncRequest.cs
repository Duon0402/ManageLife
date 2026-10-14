using System.Text.Json;

namespace ManageLife.Models
{
    public class FinanceSyncRequest
    {
        /// <summary>ServerTime của lần đồng bộ trước; null = lấy toàn bộ.</summary>
        public DateTime? Since { get; set; }

        /// <summary>Thao tác làm trên máy chưa gửi, đúng thứ tự đã làm.</summary>
        public List<FinanceSyncOperation> Ops { get; set; } = new();
    }

    /// <summary>
    /// Một thao tác của app. Type: saveWallet, deleteWallet, reorderWallets, saveCategory, deleteCategory,
    /// reorderCategories, saveTransaction, deleteTransaction.
    /// </summary>
    public class FinanceSyncOperation
    {
        public string Type { get; set; } = null!;

        /// <summary>Id bản ghi (GUID do app sinh khi tạo).</summary>
        public string? Id { get; set; }

        /// <summary>reorderWallets/reorderCategories: thứ tự mới.</summary>
        public List<string>? Ids { get; set; }

        /// <summary>Dữ liệu lưu (SaveFinanceWalletRequest, SaveFinanceCategoryRequest hoặc SaveFinanceTransactionRequest theo Type).</summary>
        public JsonElement? Body { get; set; }
    }
}
