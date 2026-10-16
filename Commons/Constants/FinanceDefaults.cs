using System.Linq;
using System.Reflection;
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
            ("food", "Ăn uống", "silverware-fork-knife", "#F97316"),
            ("transport", "Đi lại", "car-outline", "#3B82F6"),
            ("housing", "Nhà ở", "home-outline", "#8B5CF6"),
            ("bills", "Hoá đơn", "file-document-outline", "#0EA5E9"),
            ("shopping", "Mua sắp", "shopping-outline", "#EC4899"),
            ("fun", "Giải trí", "movie-open-outline", "#EAB308"),
            ("health", "Sức khoẻ", "heart-pulse", "#EF4444"),
            ("education", "Giáo dục", "school-outline", "#14B8A6"),
            ("family", "Gia đình", "account-group-outline", "#22C55E"),
            ("other-expense", "Khác", "dots-horizontal-circle-outline", "#64748B"),
        };

        private static readonly (string Key, string Name, string Icon, string Color)[] IncomeCategories =
        {
            ("salary", "Lương", "cash-multiple", "#16A34A"),
            ("bonus", "Thưởng", "gift-outline", "#F59E0B"),
            ("interest", "Lãi", "percent-outline", "#0891B2"),
            ("other-income", "Khác", "dots-horizontal-circle-outline", "#64748B"),
        };

        /// <summary>
        /// Key mọi danh mục tạo sẵn — nguồn duy nhất cho setting từ khoá ghi nhanh (AppConfigService) và bộ seed
        /// (SettingSeedingExtensions): thêm danh mục vào 2 mảng trên là đủ, thiếu ở đâu đó sẽ lỗi ngay lúc khởi động.
        /// </summary>
        public static readonly string[] CategoryKeys =
            ExpenseCategories.Concat(IncomeCategories).Select(item => item.Key).ToArray();

        /// <summary>
        /// Key setting từ khoá ghi nhanh cho mỗi danh mục mặc định (danh mục → setting). Sinh từ CategoryKeys nên
        /// không còn bảng khai báo tay ở AppConfigService; thiếu const trong SettingKeys.FinanceQuickAdd là lỗi ngay.
        /// </summary>
        public static IReadOnlyDictionary<string, string> QuickAddSettingKeys { get; } = BuildQuickAddSettingKeys();

        private static Dictionary<string, string> BuildQuickAddSettingKeys()
        {
            var fields = typeof(SettingKeys.FinanceQuickAdd).GetFields(BindingFlags.Public | BindingFlags.Static);
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in CategoryKeys)
            {
                var name = ToPascalCase(key);
                var field = fields.FirstOrDefault(item => item.Name == name)
                    ?? throw new InvalidOperationException($"Thiếu SettingKeys.FinanceQuickAdd.{name} cho danh mục mặc định '{key}'");
                map[key] = (string)field.GetRawConstantValue()!;
            }
            return map;
        }

        /// <summary>"other-expense" → "OtherExpense" (khớp tên const trong SettingKeys.FinanceQuickAdd).</summary>
        private static string ToPascalCase(string key) =>
            string.Join(string.Empty, key.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

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
            Color = "#2563EB",
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
