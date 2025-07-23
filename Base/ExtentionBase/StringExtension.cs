namespace ManageLife.Base
{
    public static class StringExtension
    {
        public static bool IsEmpty(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotEmpty(this string? value)
        {
            return !IsEmpty(value);
        }
    }
}
