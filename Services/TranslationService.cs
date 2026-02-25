using LinqKit;
using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _repo;
        private readonly ILanguageRepository _languageRepo;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _uow;

        public TranslationService(ITranslationRepository repo, ILanguageRepository languageRepo, ICacheService cache, IUnitOfWork uow)
        {
            _repo = repo;
            _languageRepo = languageRepo;
            _cache = cache;
            _uow = uow;
        }

        public async Task<Result> CreateTranslationAsync(CreateTranslationRequest request)
        {
            string msg;
            bool b;
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var language = await _languageRepo.FirstOrDefaultAsync(x => x.Id == request.LanguageId && x.IsDeleted == false);

                if (language == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var existing = await _repo.FirstOrDefaultAsync(x => x.LanguageId == language.Id && x.Key == request.Key);
                if (existing != null)
                {
                    msg = TranslationKey.Common.Message.DataExisted;
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<TranslationEntity>();

                b = await _repo.InsertAsync(entity);

                if (!b)
                {
                    msg = TranslationKey.Common.Message.CreateError;
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                var cacheItem = CacheSettings.Translations(language.Code);
                await _cache.RemoveAsync(cacheItem);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteTranslationAsync(DeleteTranslationRequest request)
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

                b = await _repo.DeleteAsync(entity);
                if (!b)
                {
                    return Result.DATA_NOT_DELETE;
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<Dictionary<string, string>>> GetDictionaryTranslationByLanguageCode(GetDictionaryTranslationByLanguageCodeRequest request)
        {
            string msg;
            try
            {
                var dictionary = new Dictionary<string, string>();

                if (request == null || request.LanguageCode.IsEmpty())
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error<Dictionary<string, string>>(Result.DATA_INVALID.Code, msg);
                }

                var cacheItem = CacheSettings.Translations(request.LanguageCode);

                dictionary = await _cache.TryGetValueAsync<Dictionary<string, string>>(cacheItem);

                if (dictionary.IsEmpty())
                {
                    var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode);
                    if (language != null)
                    {
                        var list = await _repo.Query()
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
                return Result.Exception<Dictionary<string, string>>(msg, ex);
            }
        }

        public async Task<Result<List<TranslationModel>>> GetListTranslationsAsync(GetListTranslationsRequest request)
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

                var entities = await _repo.Query().Where(predicate).ToListAsync();

                if (entities.IsNotEmpty())
                {
                    models = entities.MapToList<TranslationModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<TranslationModel>>(msg, ex);
            }
        }

        public async Task<Result<TranslationModel>> GetTranslationByIdAsync(GetTranslationByIdRequest request)
        {
            try
            {
                if (request?.Id == null) return Result.Error<TranslationModel>(Result.DATA_INVALID.Code, TranslationKey.Common.Message.DataInvalid);

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false);
                if (entity == null) return Result.Error<TranslationModel>(Result.DATA_NOT_EXISTED.Code, TranslationKey.Common.Message.DataNotExisted);

                return Result.Ok(entity.MapTo<TranslationModel>());
            }
            catch (Exception ex)
            {
                return Result.Exception<TranslationModel>(TranslationKey.Common.Message.SystemError, ex);
            }
        }

        public async Task<Result<TranslationModel>> GetTranslationByKeyAsync(GetTranslationByKeyRequest request)
        {
            string msg;
            try
            {
                if (request == null || request.LanguageCode.IsEmpty() || request.Key.IsEmpty())
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error<TranslationModel>(Result.DATA_INVALID.Code, msg);
                }

                var language = await _languageRepo.FirstOrDefaultAsync(l => l.Code == request.LanguageCode);
                if (language == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error<TranslationModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(
                    x => x.LanguageId == language.Id && x.Key == request.Key);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error<TranslationModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<TranslationModel>();
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<TranslationModel>(msg, ex);
            }
        }

        public async Task<Result> ImportTranslationExcelAsync(ImportTranslationExcelRequest request)
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
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var excelStream = request.File.OpenReadStream();
                var data = ExcelHelper.Import<ImportTranslationExcelModel>(excelStream);

                if (data.IsEmpty())
                {
                    msg = "Không tìm thấy data import";
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var languages = await _languageRepo.Query().Where(l => l.IsDeleted == false).ToListAsync();
                var allKeys = data.Select(x => x.Key).Distinct().ToList();
                
                var existingEntities = await _repo.Query()
                    .Where(x => allKeys.Contains(x.Key))
                    .ToListAsync();

                var insertEntities = new List<TranslationEntity>();
                var updateEntities = new List<TranslationEntity>();

                foreach (var item in data)
                {
                    var lang = languages.FirstOrDefault(l => l.Code == item.Language || l.Name == item.Language);
                    if (lang == null) continue;

                    var existing = existingEntities.FirstOrDefault(x => x.LanguageId == lang.Id && x.Key == item.Key);
                    if (existing != null)
                    {
                        if (existing.Value != item.Value)
                        {
                            existing.Value = item.Value;
                            updateEntities.Add(existing);
                        }
                    }
                    else
                    {
                        insertEntities.Add(new TranslationEntity
                        {
                            LanguageId = lang.Id,
                            Key = item.Key,
                            Value = item.Value
                        });
                    }
                }

                if (insertEntities.IsNotEmpty())
                {
                    b = await _repo.BulkInsertAsync(insertEntities);
                    if (!b)
                    {
                        await _uow.RollbackAsync();
                        msg = TranslationKey.Common.Message.CreateError;
                        return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                    }
                }

                if (updateEntities.IsNotEmpty())
                {
                    b = await _repo.BulkUpdateAsync(updateEntities);
                    if (!b)
                    {
                        await _uow.RollbackAsync();
                        msg = TranslationKey.Common.Message.UpdateError;
                        return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                    }
                }

                await _uow.CommitAsync();

                // Clear all translation cache
                foreach (var lang in languages)
                {
                    await _cache.RemoveAsync(CacheSettings.Translations(lang.Code));
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> UpdateTranslationAsync(UpdateTranslationRequest request)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                {
                    var msg = string.Join("\n", validation.Errors.Select(e => $"- {e}"));
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDeleted == false);
                if (entity == null) return Result.DATA_NOT_EXISTED;

                var language = await _languageRepo.FirstOrDefaultAsync(x => x.Id == request.LanguageId && x.IsDeleted == false);
                if (language == null) return Result.Error(Result.DATA_INVALID.Code, TranslationKey.Common.Message.DataInvalid);

                request.MapTo(entity);
                var b = await _repo.UpdateAsync(entity);

                if (!b) return Result.Error(Result.DATA_NOT_UPDATE.Code, TranslationKey.Common.Message.UpdateError);

                await _cache.RemoveAsync(CacheSettings.Translations(language.Code));

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception(TranslationKey.Common.Message.SystemError, ex);
            }
        }
    }
}
