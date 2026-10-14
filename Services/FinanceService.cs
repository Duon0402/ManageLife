using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    /// <summary>
    /// Áp dụng từng thao tác tài chính của app (lưu ví/danh mục/giao dịch, xoá, sắp xếp). Mọi bản ghi đều thuộc về
    /// người dùng đang đăng nhập và chưa bị xoá — không có global query filter nên mọi query tự kiểm.
    /// </summary>
    public class FinanceService : ServiceBase<FinanceService>, IFinanceService
    {
        private const string NoUserMessage = "Không xác định được người dùng";
        private const string DataDeletedMessage = "Dữ liệu đã bị xoá";
        private const string WalletNotFoundMessage = "Không tìm thấy ví";
        private const string CategoryNotFoundMessage = "Không tìm thấy danh mục";
        private const string WalletInUseMessage = "Ví đã có giao dịch, chỉ ẩn được";
        private const string CategoryInUseMessage = "Danh mục đã có giao dịch, chỉ ẩn được";
        private const string CategoryKindLockedMessage = "Danh mục đã có giao dịch, không đổi loại được";
        private const string SubCategoryMessage = "Chưa hỗ trợ danh mục con";

        /// <summary>Trần số tiền một giao dịch (đồng): 10^12.</summary>
        private const long MaxAmount = 1_000_000_000_000;

        /// <summary>Giao dịch không được quá khứ xa và không được xa quá 1 năm (chừa giao dịch định kỳ tạo trước).</summary>
        private static readonly DateTime MinOccurredAt = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static DateTime MaxOccurredAt() => DateTimeHelper.UtcNow().AddDays(366);

        private readonly IFinanceWalletRepository _walletRepo;
        private readonly IFinanceCategoryRepository _categoryRepo;
        private readonly IFinanceTransactionRepository _transactionRepo;
        private readonly IUnitOfWork _uow;

        public FinanceService(
            IAppLogger<FinanceService> logger,
            IUserContext userContext,
            IFinanceWalletRepository walletRepo,
            IFinanceCategoryRepository categoryRepo,
            IFinanceTransactionRepository transactionRepo,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _walletRepo = walletRepo;
            _categoryRepo = categoryRepo;
            _transactionRepo = transactionRepo;
            _uow = uow;
        }

        #region Dữ liệu mặc định

        public async Task<Result> EnsureDefaultDataAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                // Đã từng có danh mục (kể cả đã xoá hết) thì không tạo lại: người dùng chủ động xoá bộ mặc định
                if (await _categoryRepo.Query(true).AnyAsync(x => x.OwnerId == userId, ct)) return Result.Ok();

                await _uow.BeginTransactionAsync(ct);
                try
                {
                    var wallet = FinanceDefaults.CashWallet(userId);
                    var categories = FinanceDefaults.Categories(userId);

                    if (!await _walletRepo.InsertAsync(wallet, ct)
                        || !await _categoryRepo.BulkInsertAsync(categories, ct))
                    {
                        await _uow.RollbackAsync(ct);
                        return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo dữ liệu mặc định");
                    }

                    await _uow.CommitAsync(ct);
                }
                catch (DbUpdateException)
                {
                    // 2 thiết bị cùng mở tài chính lần đầu: bản sau trùng id cố định → bỏ qua, dùng bản đã tạo
                    await _uow.RollbackAsync(ct);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi tạo dữ liệu tài chính mặc định");
                return Result.Exception("Có lỗi xảy ra khi tạo dữ liệu mặc định", ex);
            }
        }

        #endregion

        #region Ví

        public async Task<Result> SaveWalletAsync(string id, SaveFinanceWalletRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                if (ClientId(id) is not { } walletId)
                    return Result.Error(Result.DATA_INVALID.Code, "Id ví không hợp lệ");

                // App tạo khi mất mạng rồi gửi lại (cùng id): đã tạo rồi thì cập nhật, không tạo trùng
                var wallet = await _walletRepo.Query(true).FirstOrDefaultAsync(x => x.Id == walletId, ct);
                if (wallet != null && (wallet.OwnerId != userId || wallet.IsDeleted))
                    return Result.Error(Result.DATA_EXISTED.Code, DataDeletedMessage);

                if (wallet == null)
                {
                    var maxSort = await _walletRepo.Query(true)
                        .Where(x => x.OwnerId == userId && !x.IsDeleted)
                        .MaxAsync(x => (int?)x.SortOrder, ct);

                    wallet = new FinanceWalletEntity
                    {
                        Id = walletId,
                        OwnerId = userId,
                        SortOrder = (maxSort ?? -1) + 1
                    };
                }

                // SortOrder và DefaultKey giữ nguyên: sắp xếp là thao tác riêng, ví mặc định vẫn nhận ra được
                wallet.Name = request.Name.Trim();
                wallet.Type = request.Type;
                wallet.Icon = request.Icon.Trim();
                wallet.Color = request.Color;
                wallet.InitialBalance = request.InitialBalance;
                wallet.IsArchived = request.IsArchived;

                await _uow.BeginTransactionAsync(ct);
                var saved = wallet.CreatedTime == default
                    ? await _walletRepo.InsertAsync(wallet, ct)
                    : await _walletRepo.UpdateAsync(wallet, ct);
                if (!saved)
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể lưu ví");
                }

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi lưu ví");
                return Result.Exception("Có lỗi xảy ra khi lưu ví", ex);
            }
        }

        public async Task<Result> DeleteWalletAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var wallet = await FindOwnedWalletAsync(id, userId, ct);
                // Không có / đã xoá: coi như xong (app gửi lại lệnh xoá khi mất phản hồi)
                if (wallet == null) return Result.Ok();

                if (await IsWalletInUseAsync(wallet.Id, ct))
                    return Result.Error(Result.DATA_INVALID.Code, WalletInUseMessage);

                if (!await _walletRepo.DeleteAsync(wallet, ct))
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá ví");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá ví");
                return Result.Exception("Có lỗi xảy ra khi xoá ví", ex);
            }
        }

        public async Task<Result> ReorderWalletsAsync(ReorderRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var ids = request.Ids.Distinct().ToList();
                var wallets = await _walletRepo.Query()
                    .Where(x => x.OwnerId == userId && !x.IsDeleted && ids.Contains(x.Id))
                    .ToListAsync(ct);

                // Id lạ hoặc của người khác bị bỏ qua
                foreach (var wallet in wallets)
                    wallet.SortOrder = ids.IndexOf(wallet.Id);

                if (wallets.IsNotEmpty() && !await _walletRepo.BulkUpdateAsync(wallets, ct))
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể sắp xếp ví");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi sắp xếp ví");
                return Result.Exception("Có lỗi xảy ra khi sắp xếp ví", ex);
            }
        }

        #endregion

        #region Danh mục

        public async Task<Result> SaveCategoryAsync(string id, SaveFinanceCategoryRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                // GĐ1 chỉ có danh mục 1 cấp
                if (request.ParentId.IsNotEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, SubCategoryMessage);

                if (ClientId(id) is not { } categoryId)
                    return Result.Error(Result.DATA_INVALID.Code, "Id danh mục không hợp lệ");

                var category = await _categoryRepo.Query(true).FirstOrDefaultAsync(x => x.Id == categoryId, ct);
                if (category != null && (category.OwnerId != userId || category.IsDeleted))
                    return Result.Error(Result.DATA_EXISTED.Code, DataDeletedMessage);

                if (category != null && category.Kind != request.Kind
                    && await IsCategoryInUseAsync(category.Id, ct))
                    return Result.Error(Result.DATA_INVALID.Code, CategoryKindLockedMessage);

                if (category == null)
                {
                    var maxSort = await _categoryRepo.Query(true)
                        .Where(x => x.OwnerId == userId && !x.IsDeleted && x.Kind == request.Kind)
                        .MaxAsync(x => (int?)x.SortOrder, ct);

                    category = new FinanceCategoryEntity
                    {
                        Id = categoryId,
                        OwnerId = userId,
                        Kind = request.Kind,
                        SortOrder = (maxSort ?? -1) + 1
                    };
                }

                category.Name = request.Name.Trim();
                category.Icon = request.Icon.Trim();
                category.Color = request.Color;
                category.IsArchived = request.IsArchived;

                await _uow.BeginTransactionAsync(ct);
                var saved = category.CreatedTime == default
                    ? await _categoryRepo.InsertAsync(category, ct)
                    : await _categoryRepo.UpdateAsync(category, ct);
                if (!saved)
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể lưu danh mục");
                }

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi lưu danh mục");
                return Result.Exception("Có lỗi xảy ra khi lưu danh mục", ex);
            }
        }

        public async Task<Result> DeleteCategoryAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var category = await FindOwnedCategoryAsync(id, userId, ct);
                if (category == null) return Result.Ok();

                if (await IsCategoryInUseAsync(category.Id, ct))
                    return Result.Error(Result.DATA_INVALID.Code, CategoryInUseMessage);

                if (!await _categoryRepo.DeleteAsync(category, ct))
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá danh mục");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá danh mục");
                return Result.Exception("Có lỗi xảy ra khi xoá danh mục", ex);
            }
        }

        public async Task<Result> ReorderCategoriesAsync(ReorderRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var ids = request.Ids.Distinct().ToList();
                var categories = await _categoryRepo.Query()
                    .Where(x => x.OwnerId == userId && !x.IsDeleted && ids.Contains(x.Id))
                    .ToListAsync(ct);

                foreach (var category in categories)
                    category.SortOrder = ids.IndexOf(category.Id);

                if (categories.IsNotEmpty() && !await _categoryRepo.BulkUpdateAsync(categories, ct))
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể sắp xếp danh mục");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi sắn xếp danh mục");
                return Result.Exception("Có lỗi xảy ra khi sắp xếp danh mục", ex);
            }
        }

        #endregion

        #region Giao dịch

        public async Task<Result> SaveTransactionAsync(string id, SaveFinanceTransactionRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                if (ClientId(id) is not { } transactionId)
                    return Result.Error(Result.DATA_INVALID.Code, "Id giao dịch không hợp lệ");

                var ruleError = await ValidateTransactionAsync(request, userId, ct);
                if (ruleError != null) return ruleError;

                var transaction = await _transactionRepo.Query(true).FirstOrDefaultAsync(x => x.Id == transactionId, ct);
                if (transaction != null && (transaction.OwnerId != userId || transaction.IsDeleted))
                    return Result.Error(Result.DATA_EXISTED.Code, DataDeletedMessage);

                transaction ??= new FinanceTransactionEntity { Id = transactionId, OwnerId = userId };

                transaction.Kind = request.Kind;
                transaction.Amount = request.Amount;
                transaction.WalletId = request.WalletId;
                transaction.ToWalletId = request.ToWalletId;
                transaction.CategoryId = request.CategoryId;
                transaction.OccurredAt = ToUtc(request.OccurredAt);
                transaction.Note = request.Note.IsEmpty() ? null : request.Note!.Trim();

                await _uow.BeginTransactionAsync(ct);
                var saved = transaction.CreatedTime == default
                    ? await _transactionRepo.InsertAsync(transaction, ct)
                    : await _transactionRepo.UpdateAsync(transaction, ct);
                if (!saved)
                {
                    await _uow.RollbackAsync(ct);
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể lưu giao dịch");
                }

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Lỗi khi lưu giao dịch");
                return Result.Exception("Có lỗi xảy ra khi lưu giao dịch", ex);
            }
        }

        public async Task<Result> DeleteTransactionAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, NoUserMessage);

                var transaction = await _transactionRepo.FirstOrDefaultAsync(
                    x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);
                if (transaction == null) return Result.Ok();

                if (!await _transactionRepo.DeleteAsync(transaction, ct))
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xoá giao dịch");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá giao dịch");
                return Result.Exception("Có lỗi xảy ra khi xoá giao dịch", ex);
            }
        }

        /// <summary>Kiểm giao dịch trước khi lưu (cùng luật với app, xem đặc tả T06).</summary>
        private async Task<Result?> ValidateTransactionAsync(SaveFinanceTransactionRequest request, string userId, CancellationToken ct)
        {
            var kind = request.Kind;
            var amount = request.Amount;

            if (amount is < -MaxAmount or > MaxAmount)
                return Result.Error(Result.DATA_INVALID.Code, "Số tiền không hợp lệ");

            if (kind is FinanceTransactionKind.Expense or FinanceTransactionKind.Income or FinanceTransactionKind.Transfer
                ? amount <= 0
                : amount == 0)
                return Result.Error(Result.DATA_INVALID.Code, "Số tiền không hợp lệ");

            if (!await IsOwnedWalletAsync(request.WalletId, userId, ct))
                return Result.Error(Result.DATA_NOT_EXISTED.Code, WalletNotFoundMessage);

            var occurredAt = ToUtc(request.OccurredAt);
            if (occurredAt < MinOccurredAt || occurredAt > MaxOccurredAt())
                return Result.Error(Result.DATA_INVALID.Code, "Thời điểm giao dịch không hợp lệ");

            switch (kind)
            {
                case FinanceTransactionKind.Expense:
                case FinanceTransactionKind.Income:
                    if (request.ToWalletId.IsNotEmpty())
                        return Result.Error(Result.DATA_INVALID.Code, "Thu/chi không chuyển giữa 2 ví được");
                    if (request.CategoryId.IsEmpty())
                        return Result.Error(Result.DATA_INVALID.Code, "Cần chọn danh mục cho giao dịch thu/chi");
                    if (!await IsOwnedCategoryAsync(request.CategoryId, userId, kind == FinanceTransactionKind.Expense
                            ? FinanceCategoryKind.Expense
                            : FinanceCategoryKind.Income, ct))
                        return Result.Error(Result.DATA_INVALID.Code, "Danh mục không đúng loại giao dịch");
                    break;

                case FinanceTransactionKind.Transfer:
                    if (request.CategoryId.IsNotEmpty())
                        return Result.Error(Result.DATA_INVALID.Code, "Chuyển tiền không cần danh mục");
                    if (request.ToWalletId.IsEmpty())
                        return Result.Error(Result.DATA_INVALID.Code, "Cần chọn ví đích khi chuyển tiền");
                    if (request.ToWalletId == request.WalletId)
                        return Result.Error(Result.DATA_INVALID.Code, "Ví đích phải khác ví nguồn");
                    if (!await IsOwnedWalletAsync(request.ToWalletId, userId, ct))
                        return Result.Error(Result.DATA_NOT_EXISTED.Code, WalletNotFoundMessage);
                    break;

                default:
                    if (request.ToWalletId.IsNotEmpty() || request.CategoryId.IsNotEmpty())
                        return Result.Error(Result.DATA_INVALID.Code, "Điều chỉnh số dư không cần ví đích và danh mục");
                    break;
            }

            return null;
        }

        #endregion

        #region Helpers

        /// <summary>Id app gửi lên (GUID) chuẩn hoá về dạng chữ thường; không hợp lệ/không có thì null.</summary>
        private static string? ClientId(string? id) => Guid.TryParse(id, out var guid) ? guid.ToString() : null;

        private Task<FinanceWalletEntity?> FindOwnedWalletAsync(string id, string userId, CancellationToken ct) =>
            _walletRepo.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);

        private Task<FinanceCategoryEntity?> FindOwnedCategoryAsync(string id, string userId, CancellationToken ct) =>
            _categoryRepo.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId && !x.IsDeleted, ct);

        /// <summary>Ví của người dùng, chưa xoá (ví đã ẩn vẫn dùng được: sửa giao dịch cũ).</summary>
        private Task<bool> IsOwnedWalletAsync(string? walletId, string userId, CancellationToken ct) =>
            _walletRepo.Query(true).AnyAsync(x => x.Id == walletId && x.OwnerId == userId && !x.IsDeleted, ct);

        private Task<bool> IsOwnedCategoryAsync(string? categoryId, string userId, FinanceCategoryKind kind, CancellationToken ct) =>
            _categoryRepo.Query(true).AnyAsync(x => x.Id == categoryId && x.OwnerId == userId && !x.IsDeleted && x.Kind == kind, ct);

        private Task<bool> IsWalletInUseAsync(string walletId, CancellationToken ct) =>
            _transactionRepo.Query(true).AnyAsync(
                x => !x.IsDeleted && (x.WalletId == walletId || x.ToWalletId == walletId), ct);

        private Task<bool> IsCategoryInUseAsync(string categoryId, CancellationToken ct) =>
            _transactionRepo.Query(true).AnyAsync(x => !x.IsDeleted && x.CategoryId == categoryId, ct);

        // Hợp đồng: OccurredAt là UTC. Chuỗi không có múi giờ (Kind Unspecified) coi là UTC thay vì giờ máy chủ.
        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value.ToUniversalTime()
        };

        #endregion
    }
}
