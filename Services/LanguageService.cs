using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Extentions;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class LanguageService : ServiceBase, ILanguageService
    {
        private readonly LanguageRespository _repo;

        public LanguageService(AppDbContext context) : base(context)
        {
            _repo = new LanguageRespository(context);
        }

        public async Task<Result> CreateLanguageAsync(CreateLanguageRequest request)
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

                var existing = await _repo.GetAsync(x => x.Code == request.Code && x.IsDeleted == false);
                if (existing != null)
                {
                    msg = TranslationKey.Common.Message.DataExisted;
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                var entity = request.MapTo<LanguageEntity>();

                b = await _repo.InsertAsync(entity);
                if (!b)
                {
                    msg = TranslationKey.Common.Message.CreateError;
                    return Result.Error(Result.DATA_NOT_CREATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result> DeleteLanguageAsync(DeleteLanguageRequest request)
        {
            string msg;
            bool b;
            try
            {
                if (request == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Id == request.Id && x.IsDeleted == false);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                b = await _repo.DeleteAsync(entity);

                if (!b)
                {
                    msg = TranslationKey.Common.Message.DeleteError;
                    return Result.Error(Result.DATA_NOT_DELETE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception(msg, ex);
            }
        }

        public async Task<Result<LanguageModel>> GetLanguageByCodeAsync(GetLanguageByCodeRequest request)
        {
            string msg;
            try
            {
                if (request == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error<LanguageModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Code == request.LanguageCode && x.IsDeleted == false);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error<LanguageModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<LanguageModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<LanguageModel>(msg, ex);
            }
        }

        public async Task<Result<LanguageModel>> GetLanguageByIdAsync(GetLanguageByIdRequest request)
        {
            string msg;
            try
            {
                if (request == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error<LanguageModel>(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Id == request.Id && x.IsDeleted == false);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error<LanguageModel>(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var model = entity.MapTo<LanguageModel>();

                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<LanguageModel>(msg, ex);
            }
        }

        public async Task<Result<List<LanguageModel>>> GetListLanguagesAsync()
        {
            string msg;
            try
            {
                var models = new List<LanguageModel>();

                var entities = await _repo.FindAsync(x => x.IsDeleted == false);

                if (entities.IsNotEmpty())
                    models = entities.MapToList<LanguageModel>();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<LanguageModel>>(msg, ex);
            }
        }

        public async Task<Result> UpdateLanguageAsync(UpdateLanguageRequest request)
        {
            string msg;
            bool b;
            try
            {
                if (request == null)
                {
                    msg = TranslationKey.Common.Message.DataInvalid;
                    return Result.Error(Result.DATA_INVALID.Code, msg);
                }

                var entity = await _repo.GetAsync(x => x.Id == request.Id && x.IsDeleted == false);

                if (entity == null)
                {
                    msg = TranslationKey.Common.Message.DataNotExisted;
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                var duplicate = await _repo.GetAsync(x => x.Code == request.Code && x.Id != request.Id && x.IsDeleted == false);

                if (duplicate != null)
                {
                    msg = TranslationKey.Common.Message.DataExisted;
                    return Result.Error(Result.DATA_EXISTED.Code, msg);
                }

                request.MapTo(entity);

                b = await _repo.UpdateAsync(entity);

                if (!b)
                {
                    msg = TranslationKey.Common.Message.UpdateError;
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, msg);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<LanguageModel>(msg, ex);
            }
        }
    }
}
