using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ManageLife.Core
{
    public static class StringExtension
    {
        public static bool IsEmpty([NotNullWhen(false)] this string? value)
            => string.IsNullOrWhiteSpace(value);

        public static bool IsNotEmpty([NotNullWhen(true)] this string? value)
            => !string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Chuẩn hóa chuỗi về Unicode NFC và trim khoảng trắng hai đầu.
        /// </summary>
        public static string ToNormalized(this string? value)
            => value?.Normalize(NormalizationForm.FormC).Trim() ?? string.Empty;

        /// <summary>
        /// So sánh hai chuỗi sau khi chuẩn hóa Unicode NFC và trim, mặc định không phân biệt hoa thường.
        /// </summary>
        public static bool NormalizedEquals(this string? value, string? other,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
            => string.Equals(
                value?.Normalize(NormalizationForm.FormC).Trim(),
                other?.Normalize(NormalizationForm.FormC).Trim(),
                comparison);
    }
}
