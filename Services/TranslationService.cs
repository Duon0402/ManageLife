using LinqKit;
using ManageLife.Commons;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ManageLife.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _repo;
        private readonly ILanguageRepository _languageRepo;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _uow;
        private readonly IAppLogger<TranslationService> _logger;

        public TranslationService(ITranslationRepository repo, ILanguageRepository languageRepo, ICacheService cache, IUnitOfWork uow, IAppLogger<TranslationService> logger)
        {
            _repo = repo;
            _languageRepo = languageRepo;
            _cache = cache;
            _uow = uow;
            _logger = logger;
        }

        public async Task<Result> CreateTranslationAsync(CreateTranslationRequest request, CancellationToken ct = default)
        {
            string msg;
            bool b;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var language = await _languageRepo.FirstOrDefaultAsync(x => x.Id == request.LanguageId && x.IsDeleted == false);

                if (language == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var existing = await _repo.FirstOrDefaultAsync(x => x.LanguageId == language.Id && x.Key == request.Key);
                if (existing != null)
                {
                    msg = TranslationKey.Common.Message.DataExisted;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<TranslationEntity>();

                b = await _repo.InsertAsync(entity);

                if (!b)
                {
                    msg = TranslationKey.Common.Message.CreateError;
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var cacheItem = CacheSettings.Translations(language.Code);
                await _cache.RemoveAsync(cacheItem);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteTranslationAsync(DeleteTranslationRequest request, CancellationToken ct = default)
        {
            string msg;
            bool b;
            try
            {
                if (request?.Id == null)
                {
                    return Result.DATA_INVALID;
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false);
                if (entity == null)
                {
                    return Result.DATA_NOT_EXISTED;
                }

                var language = await _languageRepo.FirstOrDefaultAsync(x => x.Id == entity.LanguageId);

                b = await _repo.DeleteAsync(entity);
                if (!b)
                {
                    return Result.DATA_NOT_DELETE;
                }

                if (language != null)
                {
                    await _cache.RemoveAsync(CacheSettings.Translations(language.Code));
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<byte[]>> DownloadTranslationTemplateExcelAsync(CancellationToken ct = default)
        {
            string msg;
            try
            {
                var languages = await _languageRepo.Query(true)
                    .Where(x => x.IsDeleted == false)
                    .Select(x => x.Name)
                    .ToListAsync(ct);

                if (languages.IsEmpty())
                {
                    msg = "Không có ngôn ngữ nào để tải template";
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
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<byte[]>(msg, ex);
            }
        }

        public async Task<Result<Dictionary<string, string>>> GetDictionaryTranslationByLanguageCode(GetDictionaryTranslationByLanguageCodeRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var dictionary = new Dictionary<string, string>();

                if (request == null || request.LanguageCode.IsEmpty())
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error<Dictionary<string, string>>(Result.DATA_INVALID.Code, msg);
                }

                var cacheItem = CacheSettings.Translations(request.LanguageCode);

                dictionary = await _cache.TryGetValueAsync<Dictionary<string, string>>(cacheItem);

                if (dictionary.IsEmpty())
                {
                    var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode);
                    if (language != null)
                    {
                        var list = await _repo.Query(true)
                            .Where(t => t.LanguageId == language.Id)
                            .ToListAsync();
                        dictionary = list.ToDictionary(t => t.Key, t => t.Value);
                    }
                    else
                    {
                        dictionary = new Dictionary<string, string>();
                    }

                    if (dictionary.IsNotEmpty())
                    {
                        await _cache.SetAsync(dictionary, cacheItem);
                    }
                }

                return Result.Ok(dictionary);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<Dictionary<string, string>>(msg, ex);
            }
        }

        public async Task<Result<List<TranslationModel>>> GetListTranslationsAsync(GetListTranslationsRequest request, CancellationToken ct = default)
        {
            string msg;
            try
            {
                var models = new List<TranslationModel>();

                var predicate = PredicateBuilder.New<TranslationEntity>(x => x.IsDeleted == false);

                if (request?.LanguageCode.IsNotEmpty() == true)
                {
                    var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode);
                    if (language != null)
                    {
                        predicate = predicate.And(x => x.LanguageId == language.Id);
                    }
                    else
                    {
                        return Result.Ok(new List<TranslationModel>());
                    }
                }

                var entities = await _repo.Query(true).Where(predicate).ToListAsync();

                if (entities.IsNotEmpty())
                {
                    models = entities.MapToList<TranslationModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
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
            string msg;
            try
            {
                if (request == null || request.LanguageCode.IsEmpty() || request.Key.IsEmpty())
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    _logger.Debug(msg);
                    return Result.Error<TranslationModel>(Result.DATA_INVALID.Code, msg);
                }

                var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode);
                if (language == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<TranslationModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(
                    x => x.LanguageId == language.Id && x.Key == request.Key);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    _logger.Debug(msg);
                    return Result.Error<TranslationModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<TranslationModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception<TranslationModel>(msg, ex);
            }
        }

        public async Task<Result> ImportTranslationExcelAsync(ImportTranslationExcelRequest request, CancellationToken ct = default)
        {
            string msg;
            bool b;
            await _uow.BeginTransactionAsync();
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var excelStream = request.File.OpenReadStream();
                var data = ExcelHelper.Import<ImportTranslationExcelModel>(excelStream);

                if (data.IsEmpty())
                {
                    msg = "Không tìm thấy data import";
                    _logger.Debug(msg);
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var languagesList = data.Select(x => x.Language).ToList();

                var existingEntities = await _repo.Query()
                    .Join(_languageRepo.Query().Where(l => languagesList.Contains(l.Name) || languagesList.Contains(l.Code)),
                        t => t.LanguageId,
                        l => l.Id,
                        (t, l) => t)
                    .ToListAsync();

                var insertEntities = new List<TranslationEntity>();
                var updateEntities = new List<TranslationEntity>();

                if (insertEntities.IsNotEmpty())
                {
                    b = await _repo.BulkInsertAsync(insertEntities);
                    if (!b)
                    {
                        msg = TranslationKey.Common.Message.CreateError;
                        _logger.Debug(msg);
                        return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                    }
                }

                if (updateEntities.IsNotEmpty())
                {
                    b = await _repo.BulkUpdateAsync(updateEntities);
                    if (!b)
                    {
                        msg = TranslationKey.Common.Message.UpdateError;
                        _logger.Debug(msg);
                        return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                    }
                }

                await _uow.CommitAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }

        public Task<Result> UpdateTranslationAsync(UpdateTranslationRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
