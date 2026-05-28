using AutoMapper;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Extensions;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class VocabTopicService : ServiceBase<VocabTopicService>, IVocabTopicService
    {
        private readonly IVocabTopicRepository _topicRepo;
        private readonly IVocabDeckRepository _deckRepo;

        public VocabTopicService(
            IAppLogger<VocabTopicService> logger,
            IUserContext userContext,
            IMapper mapper,
            IVocabTopicRepository topicRepo,
            IVocabDeckRepository deckRepo) : base(logger, userContext, mapper)
        {
            _topicRepo = topicRepo;
            _deckRepo = deckRepo;
        }

        public async Task<Result<List<VocabTopicModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var topics = await _topicRepo.Query(true)
                    .Where(t => (t.OwnerId == userId || t.IsPublic) && !t.IsDeleted)
                    .OrderBy(t => t.Name)
                    .ToListAsync(ct);

                var deckCounts = await _deckRepo.Query(true)
                    .Where(d => d.OwnerId == userId && !d.IsDeleted && d.TopicId != null)
                    .GroupBy(d => d.TopicId!)
                    .Select(g => new { TopicId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.TopicId, g => g.Count, ct);

                var models = topics.Select(t => new VocabTopicModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Color = t.Color,
                    Icon = t.Icon,
                    IsPublic = t.IsPublic,
                    DeckCount = deckCounts.TryGetValue(t.Id, out var count) ? count : 0
                }).ToList();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy danh sách topic thất bại");
                return Result.Exception<List<VocabTopicModel>>("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> CreateAsync(CreateVocabTopicRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                    return Result.Error(Result.DATA_INVALID.Code, string.Join("\n", validation.Errors.Select(e => $"- {e}")));

                var userId = _userContext.GetUserId();
                var entity = request.MapTo<VocabTopicEntity>();
                entity.OwnerId = userId;

                var inserted = await _topicRepo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Tạo topic thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Tạo topic thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateVocabTopicRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                    return Result.Error(Result.DATA_INVALID.Code, string.Join("\n", validation.Errors.Select(e => $"- {e}")));

                var userId = _userContext.GetUserId();
                var entity = await _topicRepo.FirstOrDefaultAsync(
                    t => t.Id == request.Id && t.OwnerId == userId && !t.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy topic.");

                request.MapTo<UpdateVocabTopicRequest, VocabTopicEntity>(entity);

                var updated = await _topicRepo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Cập nhật topic thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cập nhật topic thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var entity = await _topicRepo.FirstOrDefaultAsync(
                    t => t.Id == id && t.OwnerId == userId && !t.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy topic.");

                var deleted = await _topicRepo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Xóa topic thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Xóa topic thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }
    }
}
