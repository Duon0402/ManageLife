using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ManageLife.Services
{
    /// <summary>
    /// Khoá thiết bị cho đăng nhập bằng vân tay/khuôn mặt: app giữ bí mật chỉ đọc được sau khi xác thực sinh trắc
    /// (SecureStore), BE chỉ lưu hash. Đăng nhập = gửi mã thiết bị + bí mật → cấp phiên mới.
    /// </summary>
    public class DeviceCredentialService : ServiceBase<DeviceCredentialService>, IDeviceCredentialService
    {
        private const string DeviceLoginFailedMessage = "Không đăng nhập được bằng vân tay, hãy đăng nhập bằng mật khẩu";
        // Khoá thiết bị hết giá trị (bị thu hồi): app xoá bí mật trên máy và tắt tính năng
        private static readonly string CredentialRevokedCode = Result.DATA_NOT_EXISTED.Code;
        private const string NoUserMessage = "Không xác định được người dùng";

        private readonly IUserDeviceCredentialRepository _credentialRepo;
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _uow;

        public DeviceCredentialService(
            IAppLogger<DeviceCredentialService> logger,
            IUserContext userContext,
            IUserDeviceCredentialRepository credentialRepo,
            IUserRepository userRepo,
            ITokenService tokenService,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _credentialRepo = credentialRepo;
            _userRepo = userRepo;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _uow = uow;
        }

        public async Task<Result> SaveAsync(DeviceCredentialRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                // Bật lại trên máy đã có khoá: xoá bản cũ cùng mã thiết bị rồi ghi bản mới
                var stale = await _credentialRepo.Query()
                    .Where(x => x.UserId == userId && x.DeviceId == request.DeviceId)
                    .ToListAsync(ct);
                if (stale.IsNotEmpty()) await _credentialRepo.BulkDeleteAsync(stale, ct);

                var saved = await _credentialRepo.InsertAsync(new UserDeviceCredentialEntity
                {
                    Id = IdHelper.NewId(),
                    UserId = userId,
                    DeviceId = request.DeviceId,
                    DeviceName = DeviceNames.Describe(_httpContextAccessor.HttpContext?.Request),
                    SecretHash = HashSecret(request.Secret),
                    LastUsedAt = DateTimeHelper.UtcNow()
                }, ct);
                if (!saved) return Result.Error(Result.DATA_NOT_CREATE.Code, "Không bật được đăng nhập bằng vân tay");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lưu khoá thiết bị");
                return Result.Exception("Có lỗi xảy ra khi bật đăng nhập bằng vân tay", ex);
            }
        }

        public async Task<Result> DeleteAsync(string deviceId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                // Không có khoá (đã xoá ở máy khác) vẫn coi là xong
                var credentials = await _credentialRepo.Query()
                    .Where(x => x.UserId == userId && x.DeviceId == deviceId && !x.IsDeleted)
                    .ToListAsync(ct);
                if (credentials.IsNotEmpty()) await _credentialRepo.BulkDeleteAsync(credentials, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá khoá thiết bị");
                return Result.Exception("Có lỗi xảy ra khi tắt đăng nhập bằng vân tay", ex);
            }
        }

        public async Task<Result<AuthTokenModel>> LoginAsync(DeviceLoginRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, DeviceLoginFailedMessage);

                //NOTE: Tìm theo cả mã thiết bị + hash bí mật, không chỉ mã thiết bị: mã thiết bị do app gửi (không bí mật) và
                // không duy nhất giữa các tài khoản (2 tài khoản trên 1 máy, hoặc người khác cố ý đăng ký trùng mã) — tìm theo
                // mã thiết bị thôi có thể lấy nhầm bản ghi của tài khoản khác rồi báo "khoá đã thu hồi", làm app tắt vân tay oan.
                // Bí mật 256 bit ngẫu nhiên nên so khớp hash trong DB không dò được.
                var secretHash = HashSecret(request.Secret);
                var credential = await _credentialRepo.Query(true)
                    .FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId && x.SecretHash == secretHash && !x.IsDeleted, ct);
                // Khoá không còn (thu hồi khi đổi mật khẩu/đặt lại mật khẩu/đăng xuất thiết bị khác): báo mã riêng để
                // app tắt tính năng trên máy, khác với lockout tạm (mã 07) chỉ cần thử lại sau
                if (credential == null)
                    return Result.Error<AuthTokenModel>(CredentialRevokedCode, DeviceLoginFailedMessage);

                var user = await _userRepo.Query(true)
                    .FirstOrDefaultAsync(x => x.Id == credential.UserId && x.IsActive && !x.IsDeleted, ct);
                if (user == null || user.LockoutEnd > DateTimeHelper.UtcNow())
                    return Result.Error<AuthTokenModel>(Result.DATA_INVALID.Code, DeviceLoginFailedMessage);

                await _uow.BeginTransactionAsync(ct);

                credential.LastUsedAt = DateTimeHelper.UtcNow();
                credential.DeviceName = DeviceNames.Describe(_httpContextAccessor.HttpContext?.Request);
                if (!await _credentialRepo.UpdateAsync(credential, ct))
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error<AuthTokenModel>(Result.DATA_NOT_UPDATE.Code, DeviceLoginFailedMessage);
                }

                var issued = await _tokenService.IssueTokensAsync(user, ct);
                if (!issued.IsOk())
                {
                    await _uow.RollbackAsync(ct);
                    return issued;
                }

                await _uow.CommitAsync(ct);
                return issued;
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi đăng nhập bằng vân tay");
                return Result.Exception<AuthTokenModel>(DeviceLoginFailedMessage, ex);
            }
        }

        /// <summary>Hex SHA-256 của bí mật (lưu hash, không lưu bí mật gốc; đăng nhập tìm theo hash này).</summary>
        private static string HashSecret(string secret) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
    }
}
