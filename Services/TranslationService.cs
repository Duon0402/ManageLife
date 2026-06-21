using LinqKit;
using ManageLife.Commons;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Contexts;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ManageLife.Services
{
    public class TranslationService : ServiceBase<TranslationService>, ITranslationService
    {
        private readonly ITranslationRepository _repo;
        private readonly ILanguageRepository _languageRepo;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _uow;
        private readonly ITranslationFileService _fileService;

        public TranslationService(ITranslationRepository repo, ILanguageRepository languageRepo, ICacheService cache, IUnitOfWork uow, ITranslationFileService fileService, IAppLogger<TranslationService> logger, IUserContext userContext) : base(logger, userContext)
        {
            _repo = repo;
            _languageRepo = languageRepo;
            _cache = cache;
            _uow = uow;
            _fileService = fileService;
        }

        public async Task<Result> CreateTranslationAsync(CreateTranslationRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var language = await _languageRepo.FirstOrDefaultAsync(x => x.Id == request.LanguageId && x.IsDeleted == false, ct);

                if (language == null)
                {
                    var msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                request.Key = request.Key.Trim();
                request.Value = request.Value.Trim();

                var existing = await _repo.FirstOrDefaultAsync(x => x.LanguageId == language.Id && x.Key == request.Key, ct);
                if (existing != null)
                {
                    var msg = TranslationKey.Common.Message.DataExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<TranslationEntity>();

                var inserted = await _repo.InsertAsync(entity, ct);

                if (!inserted)
                {
                    var msg = TranslationKey.Common.Message.CreateError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                await InvalidateAsync(language.Code, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteTranslationAsync(DeleteTranslationRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request?.Id == null)
                {
                    return Result.DATA_INVALID;
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);
                if (entity == null)
                {
                    return Result.DATA_NOT_EXISTED;
                }

                var language = await _languageRepo.FirstOrDefaultAsync(x => x.Id == entity.LanguageId, ct);

                var deleted = await _repo.DeleteAsync(entity, ct);
                if (!deleted)
                {
                    return Result.DATA_NOT_DELETE;
                }

                if (language != null)
                    await InvalidateAsync(language.Code, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<byte[]>> DownloadTranslationTemplateExcelAsync(CancellationToken ct = default)
        {
            try
            {
                var languages = await _languageRepo.Query(true)
                    .Where(x => x.IsDeleted == false)
                    .Select(x => x.Name)
                    .ToListAsync(ct);

                if (languages.IsEmpty())
                {
                    var msg = "Không có ngôn ngữ nào để tải template";
                    _logger.Debug(msg);
                    return Result.Error<byte[]>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                using var excel = new ExcelPackage();
                var ws = excel.Workbook.Worksheets.Add("Translations");

                ws.DefaultRowHeight = 15;

                // Row 1: header (Key + tên ngôn ngữ)
                ws.Cells[1, 1].Value = "Key";

                int col = 2;
                foreach (var langName in languages)
                {
                    ws.Cells[1, col].Value = langName;
                    col++;
                }

                int totalCols = col - 1;

                // Style header row
                using (var headerRange = ws.Cells[1, 1, 1, totalCols])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(173, 216, 230));
                }

                // Freeze header, dữ liệu bắt đầu từ row 2
                ws.View.FreezePanes(2, 2);

                ws.Cells.AutoFitColumns();

                return Result.Ok(excel.GetAsByteArray());
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<byte[]>(msg, ex);
            }
        }

        public async Task<Result<Dictionary<string, string>>> GetDictionaryTranslationByLanguageCode(GetDictionaryTranslationByLanguageCodeRequest request, CancellationToken ct = default)
        {
            try
            {
                var dictionary = new Dictionary<string, string>();

                if (request == null || request.LanguageCode.IsEmpty())
                {
                    var msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error<Dictionary<string, string>>(Result.DATA_INVALID.Code, msg);
                }

                dictionary = await _fileService.ReadAsync(request.LanguageCode, ct) ?? new Dictionary<string, string>();

                if (dictionary.IsEmpty())
                {
                    var cacheItem = CacheSettings.Translations(request.LanguageCode);
                    dictionary = await _cache.TryGetValueAsync<Dictionary<string, string>>(cacheItem) ?? new Dictionary<string, string>();

                    if (dictionary.IsEmpty())
                    {
                        var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode, ct);
                        if (language != null)
                        {
                            var list = await _repo.Query(true)
                                .Where(t => t.LanguageId == language.Id)
                                .ToListAsync(ct);
                            dictionary = list.ToDictionary(t => t.Key, t => t.Value);
                        }

                        if (dictionary.IsNotEmpty())
                        {
                            await _cache.SetAsync(dictionary, cacheItem);
                            await _fileService.RegenerateAsync(request.LanguageCode, ct);
                        }
                    }
                }

                return Result.Ok(dictionary);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<Dictionary<string, string>>(msg, ex);
            }
        }

        public async Task<Result<List<TranslationModel>>> GetListTranslationsAsync(GetListTranslationsRequest request, CancellationToken ct = default)
        {
            try
            {
                var models = new List<TranslationModel>();

                var predicate = PredicateBuilder.New<TranslationEntity>(x => x.IsDeleted == false);

                if (request?.LanguageCode.IsNotEmpty() == true)
                {
                    var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode, ct);
                    if (language != null)
                    {
                        predicate = predicate.And(x => x.LanguageId == language.Id);
                    }
                    else
                    {
                        return Result.Ok(new List<TranslationModel>());
                    }
                }

                var entities = await _repo.Query(true).Where(predicate).ToListAsync(ct);

                if (entities.IsNotEmpty())
                {
                    models = entities.MapToList<TranslationModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<List<TranslationModel>>(msg, ex);
            }
        }

        public Task<Result<TranslationModel>> GetTranslationByIdAsync(GetTranslationByIdRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<TranslationModel>> GetTranslationByKeyAsync(GetTranslationByKeyRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request == null || request.LanguageCode.IsEmpty() || request.Key.IsEmpty())
                {
                    var msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error<TranslationModel>(Result.DATA_INVALID.Code, msg);
                }

                var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode, ct);
                if (language == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<TranslationModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(
                    x => x.LanguageId == language.Id && x.Key == request.Key, ct);

                if (entity == null)
                {
                    var msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<TranslationModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<TranslationModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<TranslationModel>(msg, ex);
            }
        }

        public async Task<Result> ImportTranslationExcelAsync(ImportTranslationExcelRequest request, CancellationToken ct = default)
        {
            await _uow.BeginTransactionAsync(ct);
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                using var excelStream = request.File.OpenReadStream();
                var rows = ExcelHelper.ImportAsRows(excelStream);

                if (rows.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không tìm thấy dữ liệu trong file");

                var langNames = rows[0].Keys
                    .Where(k => !string.Equals(k, "Key", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var languages = await _languageRepo.Query(true)
                    .Where(l => (langNames.Contains(l.Name) || langNames.Contains(l.Code)) && l.IsDeleted == false)
                    .ToListAsync(ct);

                if (languages.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không tìm thấy ngôn ngữ hợp lệ trong file");

                var languageIds = languages.Select(l => l.Id).ToList();
                var existingTranslations = await _repo.Query()
                    .Where(t => languageIds.Contains(t.LanguageId) && t.IsDeleted == false)
                    .ToListAsync(ct);

                var existingLookup = existingTranslations.ToDictionary(t => (t.Key, t.LanguageId));

                var insertEntities = new List<TranslationEntity>();
                var updateEntities = new List<TranslationEntity>();

                foreach (var row in rows)
                {
                    if (!row.TryGetValue("Key", out var key) || key.IsEmpty()) continue;
                    key = key.Trim();

                    foreach (var language in languages)
                    {
                        var colName = langNames.FirstOrDefault(n =>
                            string.Equals(n, language.Name, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(n, language.Code, StringComparison.OrdinalIgnoreCase));

                        if (colName == null || !row.TryGetValue(colName, out var value) || value.IsEmpty()) continue;
                        value = value.Trim();

                        if (existingLookup.TryGetValue((key, language.Id), out var existing))
                        {
                            existing.Value = value;
                            updateEntities.Add(existing);
                        }
                        else
                        {
                            insertEntities.Add(new TranslationEntity { Key = key, Value = value, LanguageId = language.Id });
                        }
                    }
                }

                if (insertEntities.IsNotEmpty() && !await _repo.BulkInsertAsync(insertEntities, ct))
                    return Result.Error(Result.DATA_NOT_CREATE.Code, TranslationKey.Common.Message.CreateError);

                if (updateEntities.IsNotEmpty() && !await _repo.BulkUpdateAsync(updateEntities, ct))
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, TranslationKey.Common.Message.UpdateError);

                await _uow.CommitAsync(ct);
                await _fileService.RegenerateAllAsync(ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateTranslationAsync(UpdateTranslationRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false, ct);
                if (entity == null) return Result.DATA_NOT_EXISTED;

                var language = await _languageRepo.FirstOrDefaultAsync(x => x.Id == request.LanguageId && x.IsDeleted == false, ct);
                if (language == null) return Result.Error(Result.DATA_INVALID.Code, TranslationKey.Common.Message.DataInvalid);

                request.Key = request.Key.Trim();
                request.Value = request.Value.Trim();

                var duplicate = await _repo.FirstOrDefaultAsync(
                    x => x.Key == request.Key && x.LanguageId == request.LanguageId && x.Id != request.Id && x.IsDeleted == false, ct);
                if (duplicate != null) return Result.Error(Result.DATA_EXISTED.Code, TranslationKey.Common.Message.DataExisted);

                entity.Key = request.Key;
                entity.Value = request.Value;
                entity.LanguageId = request.LanguageId;

                if (!await _repo.UpdateAsync(entity, ct))
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, TranslationKey.Common.Message.UpdateError);

                await InvalidateAsync(language.Code, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                var msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        private async Task InvalidateAsync(string languageCode, CancellationToken ct = default)
        {
            await _cache.RemoveAsync(CacheSettings.Translations(languageCode));
            await _fileService.RegenerateAsync(languageCode, ct);
        }
    }
}
