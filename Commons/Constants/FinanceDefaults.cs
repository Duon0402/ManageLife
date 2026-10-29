using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ManageLife.Entities;

namespace ManageLife.Commons
{
    /// <summary>
    /// Ví "Tiền mặt" và bộ danh mục thu/chi tạo sẵn ở lần đồng bộ đầu. Id sinh từ (người dùng, key) nên 2 máy cùng tạo
    /// không bị trùng; mỗi người dùng có bộ riêng, sửa/xoá được.
    /// </summary>
    public static class FinanceDefaults
    {
        public const string CashKey = "cash";

        private static readonly (string Key, string Name, string Icon, string Color)[] ExpenseCategories =
        {
            ("food", "Ăn uống", "silverware-fork-knife", "#C2662D"),
            ("transport", "Đi lại", "car-outline", "#3D6E9E"),
            ("housing", "Nhà ở", "home-outline", "#7A4C69"),
            ("bills", "Hoá đơn", "file-document-outline", "#2E7F8A"),
            ("shopping", "Mua sắm", "shopping-outline", "#A8495A"),
            ("fun", "Giải trí", "movie-open-outline", "#B58A1E"),
            ("health", "Sức khoẻ", "heart-pulse", "#B23A2A"),
            ("education", "Giáo dục", "school-outline", "#3F7F6B"),
            ("family", "Gia đình", "account-group-outline", "#5E8C3A"),
            ("other-expense", "Khác", "dots-horizontal-circle-outline", "#6B675F"),
        };

        private static readonly (string Key, string Name, string Icon, string Color)[] IncomeCategories =
        {
            ("salary", "Lương", "cash-multiple", "#2B7349"),
            ("bonus", "Thưởng", "gift-outline", "#C08A2E"),
            ("interest", "Lãi", "percent-outline", "#356D8A"),
            ("other-income", "Khác", "dots-horizontal-circle-outline", "#6B675F"),
        };

        /// <summary>Id cố định theo người dùng và key: GUID từ 16 byte đầu của SHA256("finance:{userId}:{key}").</summary>
        public static string DeterministicId(string userId, string key)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"finance:{userId}:{key}"));
            return new Guid(bytes[..16]).ToString();
        }

        public static FinanceWalletEntity CashWallet(string userId) => new()
        {
            Id = DeterministicId(userId, CashKey),
            OwnerId = userId,
            Name = "Tiền mặt",
            Type = FinanceWalletType.Cash,
            Icon = "wallet-outline",
            Color = "#1F5C6E",
            InitialBalance = 0,
            SortOrder = 0,
            IsDefault = true,
            DefaultKey = CashKey,
        };

        public static List<FinanceCategoryEntity> Categories(string userId)
        {
            var categories = new List<FinanceCategoryEntity>();
            categories.AddRange(Build(userId, FinanceCategoryKind.Expense, ExpenseCategories));
            categories.AddRange(Build(userId, FinanceCategoryKind.Income, IncomeCategories));
            return categories;
        }

        private static IEnumerable<FinanceCategoryEntity> Build(
            string userId,
            FinanceCategoryKind kind,
            (string Key, string Name, string Icon, string Color)[] items)
        {
            return items.Select((item, index) => new FinanceCategoryEntity
            {
                Id = DeterministicId(userId, item.Key),
                OwnerId = userId,
                Kind = kind,
                Name = item.Name,
                Icon = item.Icon,
                Color = item.Color,
                SortOrder = index,
                DefaultKey = item.Key,
            });
        }
    }
}
