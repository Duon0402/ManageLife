using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class NoteService : ServiceBase<NoteService>, INoteService
    {
        private readonly INoteRepository _noteRepo;
        private readonly INoteTagRepository _tagRepo;
        private readonly INoteTagRelationRepository _relationRepo;
        private readonly INoteLinkRepository _linkRepo;
        private readonly IUnitOfWork _uow;

        public NoteService(
            IAppLogger<NoteService> logger,
            IUserContext userContext,
            INoteRepository noteRepo,
            INoteTagRepository tagRepo,
            INoteTagRelationRepository relationRepo,
            INoteLinkRepository linkRepo,
            IUnitOfWork uow) : base(logger, userContext)
        {
            _noteRepo = noteRepo;
            _tagRepo = tagRepo;
            _relationRepo = relationRepo;
            _linkRepo = linkRepo;
            _uow = uow;
        }

        public async Task<Result<List<NoteModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<List<NoteModel>>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var notes = await _noteRepo.Query(true)
                    .Where(n => n.OwnerId == userId && !n.IsDeleted)
                    .OrderByDescending(n => n.UpdatedTime ?? n.CreatedTime)
                    .Select(n => new NoteModel
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Content = n.Content,
                        CreatedTime = n.CreatedTime,
                        UpdatedTime = n.UpdatedTime
                    })
                    .ToListAsync(ct);

                return Result.Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy danh sách note thất bại");
                return Result.Exception<List<NoteModel>>("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result<NoteDetailModel>> GetByIdAsync(string id, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<NoteDetailModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = await _noteRepo.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == userId && !n.IsDeleted, ct);
                if (entity == null)
                    return Result.Error<NoteDetailModel>(Result.DATA_NOT_EXISTED.Code, "Note không tồn tại");

                var tags = await GetTagsForNoteAsync(id, ct);
                var outgoing = await GetLinkedNotesAsync(userId, id, isOutgoing: true, ct);
                var incoming = await GetLinkedNotesAsync(userId, id, isOutgoing: false, ct);

                return Result.Ok(new NoteDetailModel
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    Content = entity.Content,
                    Tags = tags,
                    CreatedTime = entity.CreatedTime,
                    UpdatedTime = entity.UpdatedTime,
                    LinkedNotes = outgoing,
                    BacklinkNotes = incoming
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy note thất bại");
                return Result.Exception<NoteDetailModel>("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> CreateAsync(CreateNoteRequest request, CancellationToken ct = default)
        {
            await _uow.BeginTransactionAsync(ct);
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = new NoteEntity
                {
                    Title = request.Title.Trim(),
                    Content = request.Content,
                    OwnerId = userId
                };

                var inserted = await _noteRepo.InsertAsync(entity, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo note");

                await SyncTagRelationsAsync(entity.Id, request.TagIds, ct);

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Tạo note thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> UpdateAsync(UpdateNoteRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = await _noteRepo.FirstOrDefaultAsync(n => n.Id == request.Id && n.OwnerId == userId && !n.IsDeleted, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Note không tồn tại");

                entity.Title = request.Title.Trim();
                entity.Content = request.Content;

                var updated = await _noteRepo.UpdateAsync(entity, ct);
                if (!updated)
                    return Result.Error(Result.DATA_NOT_UPDATE.Code, "Không thể cập nhật note");

                await SyncTagRelationsAsync(entity.Id, request.TagIds, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cập nhật note thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
        {
            await _uow.BeginTransactionAsync(ct);
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var entity = await _noteRepo.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == userId && !n.IsDeleted, ct);
                if (entity == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Note không tồn tại");

                var relations = await _relationRepo.FindAsync(r => r.NoteId == id, ct);
                if (relations.Any()) await _relationRepo.BulkDeleteAsync(relations, ct);

                var links = await _linkRepo.FindAsync(l => l.SourceNoteId == id || l.TargetNoteId == id, ct);
                if (links.Any()) await _linkRepo.BulkDeleteAsync(links, ct);

                var deleted = await _noteRepo.DeleteAsync(entity, ct);
                if (!deleted)
                    return Result.Error(Result.DATA_NOT_DELETE.Code, "Không thể xóa note");

                await _uow.CommitAsync(ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.Error(ex, "Xóa note thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> AddLinkAsync(AddNoteLinkRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                if (request.SourceNoteId == request.TargetNoteId)
                    return Result.Error(Result.DATA_INVALID.Code, "Không thể link note với chính nó");

                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var existed = await _linkRepo.FirstOrDefaultAsync(l => l.SourceNoteId == request.SourceNoteId && l.TargetNoteId == request.TargetNoteId, ct);
                if (existed != null)
                    return Result.Error(Result.DATA_EXISTED.Code, "Link đã tồn tại");

                var link = new NoteLinkEntity
                {
                    SourceNoteId = request.SourceNoteId,
                    TargetNoteId = request.TargetNoteId,
                    OwnerId = userId
                };

                var inserted = await _linkRepo.InsertAsync(link, ct);
                if (!inserted)
                    return Result.Error(Result.DATA_NOT_CREATE.Code, "Không thể tạo link");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Thêm link thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result> RemoveLinkAsync(RemoveNoteLinkRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var link = await _linkRepo.FirstOrDefaultAsync(l => l.SourceNoteId == request.SourceNoteId && l.TargetNoteId == request.TargetNoteId, ct);
                if (link == null)
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, "Link không tồn tại");

                await _linkRepo.DeleteAsync(link, ct);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Xóa link thất bại");
                return Result.Exception("Có lỗi xảy ra", ex);
            }
        }

        public async Task<Result<NoteGraphModel>> GetGraphDataAsync(CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty()) return Result.Error<NoteGraphModel>(Result.DATA_INVALID.Code, "Không xác định được người dùng");

                var notes = await _noteRepo.Query(true)
                    .Where(n => n.OwnerId == userId && !n.IsDeleted)
                    .Select(n => new { n.Id, n.Title })
                    .ToListAsync(ct);

                var noteIds = notes.Select(n => n.Id).ToList();

                var relations = await _relationRepo.Query(true)
                    .Where(r => noteIds.Contains(r.NoteId))
                    .ToListAsync(ct);

                var links = await _linkRepo.Query(true)
                    .Where(l => noteIds.Contains(l.SourceNoteId))
                    .ToListAsync(ct);

                var tagsByNote = relations
                    .GroupBy(r => r.NoteId)
                    .ToDictionary(g => g.Key, g => g.Select(r => r.TagId).ToList());

                var linkCountByNote = links
                    .GroupBy(l => l.SourceNoteId)
                    .ToDictionary(g => g.Key, g => g.Count());

                return Result.Ok(new NoteGraphModel
                {
                    Nodes = notes.Select(n => new NoteGraphNodeData
                    {
                        Id = n.Id,
                        Label = n.Title,
                        TagIds = tagsByNote.TryGetValue(n.Id, out var tags) ? tags : [],
                        LinkCount = linkCountByNote.TryGetValue(n.Id, out var cnt) ? cnt : 0
                    }).ToList(),
                    Edges = links.Select(l => new NoteGraphEdgeData
                    {
                        Source = l.SourceNoteId,
                        Target = l.TargetNoteId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy graph data thất bại");
                return Result.Exception<NoteGraphModel>("Có lỗi xảy ra", ex);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private async Task<List<NoteTagModel>> GetTagsForNoteAsync(string noteId, CancellationToken ct)
        {
            return await _relationRepo.Query(true)
                .Where(r => r.NoteId == noteId)
                .Join(_tagRepo.Query(true).Where(t => !t.IsDeleted),
                    r => r.TagId, t => t.Id,
                    (r, t) => new NoteTagModel { Id = t.Id, Name = t.Name, Color = t.Color })
                .ToListAsync(ct);
        }

        private async Task<List<NoteModel>> GetLinkedNotesAsync(string userId, string noteId, bool isOutgoing, CancellationToken ct)
        {
            var linkQuery = isOutgoing
                ? _linkRepo.Query(true).Where(l => l.SourceNoteId == noteId).Select(l => l.TargetNoteId)
                : _linkRepo.Query(true).Where(l => l.TargetNoteId == noteId).Select(l => l.SourceNoteId);

            return await _noteRepo.Query(true)
                .Where(n => linkQuery.Contains(n.Id) && n.OwnerId == userId && !n.IsDeleted)
                .Select(n => new NoteModel { Id = n.Id, Title = n.Title, CreatedTime = n.CreatedTime, UpdatedTime = n.UpdatedTime })
                .ToListAsync(ct);
        }

        private async Task SyncTagRelationsAsync(string noteId, List<string> tagIds, CancellationToken ct)
        {
            var existing = await _relationRepo.FindAsync(r => r.NoteId == noteId, ct);
            if (existing.Any()) await _relationRepo.BulkDeleteAsync(existing, ct);

            var newRelations = tagIds.Distinct()
                .Select(tagId => new NoteTagRelationEntity { NoteId = noteId, TagId = tagId })
                .ToList();

            if (newRelations.Count > 0) await _relationRepo.BulkInsertAsync(newRelations, ct);
        }
    }
}
