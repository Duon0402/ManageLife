using System.Security.Cryptography;
using System.Text;

namespace ManageLife.Helpers
{
    public class PasswordHelper
    {
        private const int WorkFactor = 12;

        public static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public static bool Verify(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);

        // BCrypt hashes bắt đầu bằng "$2a$" hoke kHA256 Base64
        public static bool IsLegacyHash(string hash)
            => !hash.StartsWith("$2");

        // Chỉ dùng trong migration path: verify SHA256 legacy hash
        internal static bool VerifyLegacy(string password, string hash)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha256.ComputeHash(bytes)) == hash;
        }
    }
}
