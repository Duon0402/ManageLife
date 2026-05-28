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
    public class VocabDeckService : ServiceBase<VocabDeckService>, IVocabDeckService
    {
        private readonly IVocabDeckRepository _deckRepo;
        private readonly IVocabDeckWordRepository _deckWordRepo;
        private readonly IVocabWordRepository _wordRepo;
        private readonly IVocabTopicRepository _topicRepo;

        public VocabDeckService(
            IAppLogger<VocabDeckService> logger,
            IUserContext userContext,
            IMapper mapper,
            IVocabDeckRepository deckRepo,
            IVocabDeckWordRepository deckWordRepo,
            IVocabWordRepository wordRepo,
            IVocabTopicRepository topicRepo) : base(logger, userContext, mapper)
        {
            _deckRepo = deckRepo;
            _deckWordRepo = deckWordRepo;
            _wordRepo = wordRepo;
            _topicRepo = topicRepo;
        }

        public async Task<Result<List<VocabDeckModel>>> GetListAsync(string? topicId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<List<VocabDeckModel>>(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var query = _deckRepo.Query(true)
                    .Where(d => d.OwnerId == userId && !d.IsDeleted);

                if (!string.IsNullOrEmpty(topicId))
                    query = query.Where(d => d.TopicId == topicId);

                var decks = await query.OrderBy(d => d.Name).ToListAsync(ct);

                var topicIds = decks.Where(d => d.TopicId != null).Select(d => d.TopicId!).Distinct().ToList();
                var topics = await _topicRepo.Query(true)
                    .Where(t => topicIds.Contains(t.Id))
                    .ToDictionaryAsync(t => t.Id, ct);

                var models = decks.Select(d => new VocabDeckModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    TopicId = d.TopicId,
                    TopicName = d.TopicId != null && topics.TryGetValue(d.TopicId, out var t) ? t.Name : null,
                    TopicColor = d.TopicId != null && topics.TryGetValue(d.TopicId, out var tc) ? tc.Color : null,
                    TotalCards = d.TotalCards,
                    CreatedTime = d.CreatedTime
                }).ToList();

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy danh sách deck thất bại");
                return Result.Exception<List<VocabDeckModel>>("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result<VocabDeckModel>> GetByIdAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<VocabDeckModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var deck = await _deckRepo.FirstOrDefaultAsync(
                    d => d.Id == id && d.OwnerId == userId && !d.IsDeleted, ct);

                if (deck == null)
                    return Result.Error<VocabDeckModel>(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy deck.");

                VocabTopicEntity? topic = null;
                if (deck.TopicId != null)
                    topic = await _topicRepo.FirstOrDefaultAsync(t => t.Id == deck.TopicId, ct);

                return Result.Ok(new VocabDeckModel
                {
                    Id = deck.Id,
                    Name = deck.Name,
                    Description = deck.Description,
                    TopicId = deck.TopicId,
                    TopicName = topic?.Name,
                    TopicColor = topic?.Color,
                    TotalCards = deck.TotalCards,
                    CreatedTime = deck.CreatedTime
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy deck thất bại");
                return Result.Exception<VocabDeckModel>("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> CreateAsync(CreateVocabDeckRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                    return Result.Error(Result.DATA_INVALID.Code, string.Join("\n", validation.Errors.Select(e => $"- {e}")));

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var entity = request.MapTo<VocabDeckEntity>();
                entity.OwnerId = userId!;
                entity.TotalCards = 0;

                var inserted = await _deckRepo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Tạo deck thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Tạo deck thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateVocabDeckRequest request, CancellationToken ct = default)
        {
            try
            {
                var validation = request.Validate();
                if (!validation.IsValid)
                    return Result.Error(Result.DATA_INVALID.Code, string.Join("\n", validation.Errors.Select(e => $"- {e}")));

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var entity = await _deckRepo.FirstOrDefaultAsync(
                    d => d.Id == request.Id && d.OwnerId == userId && !d.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy deck.");

                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.TopicId = request.TopicId;

                var updated = await _deckRepo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Cập nhật deck thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cập nhật deck thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var entity = await _deckRepo.FirstOrDefaultAsync(
                    d => d.Id == id && d.OwnerId == userId && !d.IsDeleted, ct);

                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy deck.");

                var deleted = await _deckRepo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Xóa deck thất bại.");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Xóa deck thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> AddWordAsync(AddWordToDeckRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var deck = await _deckRepo.FirstOrDefaultAsync(
                    d => d.Id == request.DeckId && d.OwnerId == userId && !d.IsDeleted, ct);
                if (deck == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy deck.");

                var word = await _wordRepo.FirstOrDefaultAsync(
                    w => w.Id == request.WordId && w.OwnerId == userId && !w.IsDeleted, ct);
                if (word == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy từ vựng.");

                var existing = await _deckWordRepo.Query(true)
                    .AnyAsync(dw => dw.DeckId == request.DeckId && dw.WordId == request.WordId, ct);
                if (existing)
                    return Result.Error(Result.DATA_EXISTED.Code, "Từ này đã có trong deck.");

                var maxOrder = await _deckWordRepo.Query(true)
                    .Where(dw => dw.DeckId == request.DeckId)
                    .Select(dw => (int?)dw.SortOrder)
                    .MaxAsync(ct) ?? 0;

                await _deckWordRepo.InsertAsync(new VocabDeckWordEntity
                {
                    DeckId = request.DeckId,
                    WordId = request.WordId,
                    SortOrder = maxOrder + 1,
                    AddedAt = DateTime.UtcNow
                }, ct);

                deck.TotalCards++;
                await _deckRepo.UpdateAsync(deck, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Thêm từ vào deck thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> RemoveWordAsync(string deckId, string wordId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var deck = await _deckRepo.FirstOrDefaultAsync(
                    d => d.Id == deckId && d.OwnerId == userId && !d.IsDeleted, ct);
                if (deck == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy deck.");

                var deckWord = await _deckWordRepo.Query(false)
                    .FirstOrDefaultAsync(dw => dw.DeckId == deckId && dw.WordId == wordId, ct);
                if (deckWord == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Từ này không có trong deck.");

                await _deckWordRepo.DeleteAsync(deckWord, ct);

                deck.TotalCards = Math.Max(0, deck.TotalCards - 1);
                await _deckRepo.UpdateAsync(deck, ct);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Xóa từ khỏi deck thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result<List<VocabWordModel>>> GetWordsAsync(string deckId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<List<VocabWordModel>>(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var deck = await _deckRepo.FirstOrDefaultAsync(
                    d => d.Id == deckId && d.OwnerId == userId && !d.IsDeleted, ct);
                if (deck == null)
                    return Result.Error<List<VocabWordModel>>(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy deck.");

                var words = await _deckWordRepo.Query(true)
                    .Where(dw => dw.DeckId == deckId)
                    .OrderBy(dw => dw.SortOrder)
                    .Join(_wordRepo.Query(true).Where(w => !w.IsDeleted),
                        dw => dw.WordId,
                        w => w.Id,
                        (dw, w) => w)
                    .ToListAsync(ct);

                return Result.Ok(words.MapToList<VocabWordEntity, VocabWordModel>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy từ vựng trong deck thất bại");
                return Result.Exception<List<VocabWordModel>>("Đã có lỗi xảy ra.", ex);
            }
        }
    }
}
