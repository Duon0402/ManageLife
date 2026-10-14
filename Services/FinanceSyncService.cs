using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ManageLife.Services
{
    /// <summary>
    /// Đồng bộ tài chính với app (app lưu trên máy, làm được khi mất mạng): 1 request gồm mọi thao tác chưa gửi, áp dụng
    /// lần lượt đúng thứ tự rồi trả thay đổi về. Thao tác bị từ chối (dữ liệu không hợp lệ, đã bị xoá...) thì bỏ qua và
    /// trả toàn bộ dữ liệu để máy về đúng như server; lỗi server thì dừng, app gửi lại phần còn lại lần sau.
    /// </summary>
    public class FinanceSyncService : ServiceBase<FinanceSyncService>, IFinanceSyncService
    {
        private const int MaxOps = 100;
        // Đồng bộ lấy chồng thêm trước mốc (giây): bù lệch thời điểm ghi giữa các request
        private const int SyncOverlapSeconds = 10;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private static readonly HashSet<string> DeleteTypes = new() { "deleteWallet", "deleteCategory", "deleteTransaction" };

        private readonly IFinanceService _financeService;
        private readonly IFinanceWalletRepository _walletRepo;
        private readonly IFinanceCategoryRepository _categoryRepo;
        private readonly IFinanceTransactionRepository _transactionRepo;
        private readonly ISettingContext _settingContext;
        private readonly IUnitOfWork _uow;

        public FinanceSyncService(
            IAppLogger<FinanceSyncService> logger,
            IUserContext userContext,
            IFinanceService financeService,
            IFinanceWalletRepository walletRepo,
            IFinanceCategoryRepository categoryRepo,
            IFinanceTransactionRepository transactionRepo,
            ISettingContext settingContext,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _financeService = financeService;
            _walletRepo = walletRepo;
            _categoryRepo = categoryRepo;
            _transactionRepo = transactionRepo;
            _settingContext = settingContext;
            _uow = uow;
        }

        public async Task<Result<FinanceSyncModel>> SyncAsync(FinanceSyncRequest request, CancellationToken ct = default)
        {
            try
            {
                // Tính năng bị tắt ở admin: app giữ thao tác lại, lần sau gửi tiếp
                if (!await _settingContext.GetBoolAsync(SettingKeys.Feature.EnableFinance, true))
                    return Result.Error<FinanceSyncModel>("FEATURE_DISABLED", "Tính năng Tài chính đang tạm tắt");

                //NOTE: JSON gửi "ops": null thì đè giá trị mặc định
                request.Ops ??= new List<FinanceSyncOperation>();
                if (request.Ops.Count > MaxOps)
                    return Result.Error<FinanceSyncModel>(Result.DATA_INVALID.Code, $"Mỗi lần đồng bộ tối đa {MaxOps} thao tác");

                var rejected = new List<string>();

                // Lần đầu mở tài chính: tạo ví "Tiền mặt" + danh mục mặc định trước khi áp thao tác của app
                var seeded = await _financeService.EnsureDefaultDataAsync(ct);
                if (seeded.IsException())
                    return Result.Error<FinanceSyncModel>(seeded.Code, seeded.Message);

                for (var i = 0; i < request.Ops.Count; i++)
                {
                    var op = request.Ops[i];
                    var rs = await ApplyAsync(op, ct);
                    //NOTE: Thao tác lỗi có thể để lại entity đã sửa trong DbContext: bỏ hết để thao tác sau không lưu nhầm
                    _uow.Context.ChangeTracker.Clear();

                    if (rs.IsException())
                        return Result.Ok(new FinanceSyncModel { Processed = i, Rejected = rejected });

                    // Gửi lại lệnh xoá (lần trước server đã xoá nhưng app không nhận được phản hồi) coi như xong
                    if (!rs.IsOk() && !(DeleteTypes.Contains(op.Type) && rs.Code == Result.DATA_NOT_EXISTED.Code))
                        rejected.Add(rs.Message);
                }

                var changes = await GetChangesAsync(rejected.Count == 0 ? request.Since : null, ct);
                if (!changes.IsOk())
                    return Result.Ok(new FinanceSyncModel { Processed = request.Ops.Count, Rejected = rejected });

                var model = changes.Data!;
                model.Processed = request.Ops.Count;
                model.Complete = true;
                model.Rejected = rejected;
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi đồng bộ tài chính");
                return Result.Exception<FinanceSyncModel>("Có lỗi xảy ra khi đồng bộ tài chính", ex);
            }
        }

        /// <summary>
        /// Thay đổi từ mốc `since` cho app lưu trên máy (không có mốc: toàn bộ). Lấy chồng thêm vài giây trước mốc để
        /// không lỡ thay đổi ghi cùng lúc với lần đồng bộ trước (nhận trùng thì app chỉ ghi đè lại).
        /// </summary>
        public async Task<Result<FinanceSyncModel>> GetChangesAsync(DateTime? since, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<FinanceSyncModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var serverTime = DateTimeHelper.UtcNow();
                var from = since.HasValue ? ToUtc(since)!.Value.AddSeconds(-SyncOverlapSeconds) : (DateTime?)null;

                var wallets = await _walletRepo.Query(true)
                    .Where(x => x.OwnerId == userId)
                    .Where(x => from == null
                        ? !x.IsDeleted
                        : x.CreatedTime >= from || x.UpdatedTime >= from || x.DeletedTime >= from)
                    .ToListAsync(ct);
                var categories = await _categoryRepo.Query(true)
                    .Where(x => x.OwnerId == userId)
                    .Where(x => from == null
                        ? !x.IsDeleted
                        : x.CreatedTime >= from || x.UpdatedTime >= from || x.DeletedTime >= from)
                    .ToListAsync(ct);
                var transactions = await _transactionRepo.Query(true)
                    .Where(x => x.OwnerId == userId)
                    .Where(x => from == null
                        ? !x.IsDeleted
                        : x.CreatedTime >= from || x.UpdatedTime >= from || x.DeletedTime >= from)
                    .ToListAsync(ct);

                return Result.Ok(new FinanceSyncModel
                {
                    ServerTime = serverTime,
                    Full = from == null,
                    Wallets = wallets.Where(x => !x.IsDeleted).MapToList<FinanceWalletEntity, FinanceWalletModel>(),
                    Categories = categories.Where(x => !x.IsDeleted).MapToList<FinanceCategoryEntity, FinanceCategoryModel>(),
                    Transactions = transactions.Where(x => !x.IsDeleted).MapToList<FinanceTransactionEntity, FinanceTransactionModel>(),
                    DeletedWalletIds = wallets.Where(x => x.IsDeleted).Select(x => x.Id).ToList(),
                    DeletedCategoryIds = categories.Where(x => x.IsDeleted).Select(x => x.Id).ToList(),
                    DeletedTransactionIds = transactions.Where(x => x.IsDeleted).Select(x => x.Id).ToList(),
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy thay đổi tài chính");
                return Result.Exception<FinanceSyncModel>("Có lỗi xảy ra khi đồng bộ tài chính", ex);
            }
        }

        private Task<Result> ApplyAsync(FinanceSyncOperation op, CancellationToken ct)
        {
            try
            {
                return op.Type switch
                {
                    "saveWallet" => _financeService.SaveWalletAsync(RequireId(op), Body<SaveFinanceWalletRequest>(op), ct),
                    "deleteWallet" => _financeService.DeleteWalletAsync(RequireId(op), ct),
                    "reorderWallets" => _financeService.ReorderWalletsAsync(new ReorderRequest { Ids = op.Ids ?? new() }, ct),
                    "saveCategory" => _financeService.SaveCategoryAsync(RequireId(op), Body<SaveFinanceCategoryRequest>(op), ct),
                    "deleteCategory" => _financeService.DeleteCategoryAsync(RequireId(op), ct),
                    "reorderCategories" => _financeService.ReorderCategoriesAsync(new ReorderRequest { Ids = op.Ids ?? new() }, ct),
                    "saveTransaction" => _financeService.SaveTransactionAsync(RequireId(op), Body<SaveFinanceTransactionRequest>(op), ct),
                    "deleteTransaction" => _financeService.DeleteTransactionAsync(RequireId(op), ct),
                    _ => Task.FromResult(Result.Error(Result.DATA_INVALID.Code, $"Thao tác không hỗ trợ: {op.Type}"))
                };
            }
            catch (Exception ex)
            {
                // Chỉ lỗi đọc dữ liệu thao tác (service tự bắt lỗi của nó): dữ liệu gửi lên sai, từ chối thao tác này
                // thay vì dừng cả lần đồng bộ
                _logger.Debug("Thao tác đồng bộ không hợp lệ: {Error}", ex.Message);
                return Task.FromResult(Result.Error(Result.DATA_INVALID.Code, ex is InvalidOperationException ? ex.Message : "Dữ liệu không hợp lệ"));
            }
        }

        private static string RequireId(FinanceSyncOperation op) =>
            op.Id.IsNotEmpty() ? op.Id! : throw new InvalidOperationException("Thiếu id");

        // Hợp đồng: Since là UTC. Chuỗi không có múi giờ (Kind Unspecified) coi là UTC thay vì giờ máy chủ.
        private static DateTime? ToUtc(DateTime? value) => value switch
        {
            null => null,
            { Kind: DateTimeKind.Unspecified } v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
            var v => v.Value.ToUniversalTime()
        };

        /// <summary>Đọc dữ liệu lưu của thao tác; thiếu/hỏng thì coi như thao tác không hợp lệ.</summary>
        private static T Body<T>(FinanceSyncOperation op) where T : class
        {
            var body = op.Body is { ValueKind: JsonValueKind.Object } element
                ? element.Deserialize<T>(JsonOptions)
                : null;
            return body ?? throw new InvalidOperationException("Thiếu dữ liệu");
        }
    }
}
