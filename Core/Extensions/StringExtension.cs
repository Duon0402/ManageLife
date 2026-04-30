using System.Diagnostics.CodeAnalysis;

namespace ManageLife.Core
{
    public static class StringExtension
    {
        public static bool IsEmpty([NotNullWhen(false)] this string? value)
            => string.IsNullOrWhiteSpace(value);

        public static bool IsNotEmpty([NotNullWhen(true)] this string? value)
            => !string.IsNullOrWhiteSpace(value);
    }
}
