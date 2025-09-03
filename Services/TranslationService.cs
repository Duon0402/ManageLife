using LinqKit;
using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Entities;
using ManageLife.Extentions;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class TranslationService : ServiceBase, ITranslationService
    {
        private readonly TranslationRepository _repo;

        public TranslationService(AppDbContext context) : base(context)
        {
            _repo = new TranslationRepository(context);
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

                var existing = await _repo.GetAsync(x => x.LanguageId == request.LanguageId && x.Key == request.Key);
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
                    msg = TranslationKey.Common.Message.InvalidData;
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

        public async Task<Result<List<TranslationModel>>> GetListTranslationsAsync(GetListTranslationsRequest request)
        {
            string msg;
            try
            {
                var models = new List<TranslationModel>();

                var predicate = PredicateBuilder.New<TranslationEntity>(x => x.IsDeleted == false);

                if (request?.LanguageCode.IsNotEmpty() == true)
                {
                    predicate = predicate.And(x => x.Language != null && x.Language.Code == request.LanguageCode);
                }

                var entities = await _repo.Query(true).Include(x => x.Language).Where(predicate).ToListAsync();

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

        public Task<Result<TranslationModel>> GetTranslationByIdAsync(GetTranslationByIdRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<TranslationModel>> GetTranslationByKeyAsync(GetTranslationByKeyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result> UpdateTranslationAsync(UpdateTranslationRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
