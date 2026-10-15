using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ManageLife.Services
{
    /// <summary>
    /// Quên mật khẩu qua Telegram (đăng ký không có email nên Telegram là kênh chính): nhập tên đăng nhập → gửi mã 6 số
    /// qua bot đã liên kết → nhập mã + mật khẩu mới → đổi mật khẩu và thu hồi mọi phiên.
    /// </summary>
    public class PasswordResetService : ServiceBase<PasswordResetService>, IPasswordResetService
    {
        private const int CodeLength = 6;
        private const int MaxAttempts = 5;
        private const int CodeLifetimeMinutes = 10;
        // Không cho spam Telegram: mã mới chỉ gửi lại sau bao lâu
        private const int ResendCooldownSeconds = 60;
        private const string SameMessage = "Nếu tài khoản đã liên kết Telegram, mã đã được gửi";
        private const string InvalidCodeMessage = "Mã không đúng hoặc đã hết hạn";
        private const string NoUserMessage = "Không xác định được người dùng";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>Mã đặt lại mật khẩu lưu trong cache: chỉ giữ hash + số lần nhập sai, không giữ mã gốc.</summary>
        private sealed class ResetCode
        {
            public string Hash { get; set; } = null!;
            public int Attempts { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private readonly IUserRepository _userRepo;
        private readonly IUserTelegramConnectionRepository _connectionRepo;
        private readonly ITelegramService _telegramService;
        private readonly ITokenService _tokenService;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _uow;

        public PasswordResetService(
            IAppLogger<PasswordResetService> logger,
            IUserContext userContext,
            IUserRepository userRepo,
            IUserTelegramConnectionRepository connectionRepo,
            ITelegramService telegramService,
            ITokenService tokenService,
            ICacheService cache,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _userRepo = userRepo;
            _connectionRepo = connectionRepo;
            _telegramService = telegramService;
            _tokenService = tokenService;
            _cache = cache;
            _uow = uow;
        }

        public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var user = await FindActiveUserAsync(request.UserName, ct);
                var connection = user == null ? null : await FindConnectionAsync(user.Id, ct);
                // Không có tài khoản hoặc chưa liên kết Telegram: vẫn trả cùng một câu, không gửi gì
                if (user == null || connection == null) return Result.Ok();

                var cacheItem = CacheSettings.PasswordResetCode(user.Id);
                var existing = await _cache.TryGetValueAsync<ResetCode>(cacheItem);
                // Mã còn hạn mà mới gửi: không gửi lại (tránh spam Telegram)
                if (existing != null && (DateTimeHelper.UtcNow() - existing.CreatedAt).TotalSeconds < ResendCooldownSeconds)
                    return Result.Ok();

                var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString($"D{CodeLength}");
                var resetCode = new ResetCode
                {
                    Hash = HashCode(user.Id, code),
                    Attempts = 0,
                    CreatedAt = DateTimeHelper.UtcNow()
                };
                await _cache.SetAsync(resetCode, cacheItem);
                //NOTE: CacheService nuốt lỗi khi ghi: đọc lại để biết mã có thực sự lưu được không
                if (await _cache.TryGetValueAsync<ResetCode>(cacheItem) == null)
                {
                    _logger.Error(new InvalidOperationException("Ghi cache mã đặt lại mật khẩu thất bại"),
                        "Không lưu được mã đặt lại mật khẩu của user {UserId}", user.Id);
                    return Result.Ok();
                }

                var sent = await _telegramService.SendMessageToChatAsync(connection.ChatId,
                    $"Mã đặt lại mật khẩu Personal Hub: {code}\nMã hết hạn sau {CodeLifetimeMinutes} phút. Không đưa mã này cho ai.", ct);
                if (!sent.IsOk()) _logger.Error(new InvalidOperationException(sent.Message), "Gửi mã đặt lại mật khẩu qua Telegram thất bại");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi tạo mã đặt lại mật khẩu");
                return Result.Exception(SameMessage, ex);
            }
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var user = await FindActiveUserAsync(request.UserName, ct);
                if (user == null) return Result.Error(Result.DATA_INVALID.Code, InvalidCodeMessage);

                var cacheItem = CacheSettings.PasswordResetCode(user.Id);
                var resetCode = await _cache.TryGetValueAsync<ResetCode>(cacheItem);
                if (resetCode == null) return Result.Error(Result.DATA_INVALID.Code, InvalidCodeMessage);

                if (!CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(HashCode(user.Id, request.Code)),
                        Encoding.UTF8.GetBytes(resetCode.Hash)))
                {
                    resetCode.Attempts += 1;
                    if (resetCode.Attempts >= MaxAttempts)
                        await _cache.RemoveAsync(cacheItem);
                    else
                        // Giữ đúng thời hạn còn lại: nhập sai không được kéo dài mã
                        await _cache.SetAsync(resetCode, CacheSettings.PasswordResetCode(
                            user.Id, TimeSpan.FromMinutes(CodeLifetimeMinutes) - (DateTimeHelper.UtcNow() - resetCode.CreatedAt)));
                    return Result.Error(Result.DATA_INVALID.Code, InvalidCodeMessage);
                }

                await _uow.BeginTransactionAsync(ct);

                user.HashPassword = PasswordHelper.HashPassword(request.NewPassword);
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                if (!await _userRepo.UpdateAsync(user, ct))
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể đặt lại mật khẩu");
                }

                // Đổi mật khẩu thì mọi phiên + khoá vân tay trên mọi máy đều bị thu hồi
                var revoked = await _tokenService.RevokeAllSessionsAsync(user, ct);
                if (!revoked.IsOk())
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(revoked.Code, revoked.Message);
                }

                await _uow.CommitAsync(ct);
                await _cache.RemoveAsync(cacheItem);

                var connection = await FindConnectionAsync(user.Id, ct);
                if (connection != null)
                    await _telegramService.SendMessageToChatAsync(connection.ChatId,
                        "Mật khẩu Personal Hub vừa được đặt lại. Nếu không phải bạn, hãy liên hệ admin.", ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi đặt lại mật khẩu");
                return Result.Exception("Có lỗi xảy ra khi đặt lại mật khẩu", ex);
            }
        }

        private Task<UserEntity?> FindActiveUserAsync(string userName, CancellationToken ct) =>
            _userRepo.FirstOrDefaultAsync(x => x.UserName == userName && x.IsActive && !x.IsDeleted, ct);

        private Task<UserTelegramConnectionEntity?> FindConnectionAsync(string userId, CancellationToken ct) =>
            _connectionRepo.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, ct);

        /// <summary>Hash của mã theo từng user: cùng một mã ở hai tài khoản cũng không dùng chung được.</summary>
        private static string HashCode(string userId, string code) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{userId}:{code}")));
    }
}
