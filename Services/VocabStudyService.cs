using AutoMapper;
using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Helpers;
using ManageLife.Interfaces;
using ManageLife.Models;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Services
{
    public class VocabStudyService : ServiceBase<VocabStudyService>, IVocabStudyService
    {
        private readonly IVocabDeckRepository _deckRepo;
        private readonly IVocabDeckWordRepository _deckWordRepo;
        private readonly IVocabWordRepository _wordRepo;
        private readonly IVocabStudyProgressRepository _progressRepo;

        public VocabStudyService(
            IAppLogger<VocabStudyService> logger,
            IUserContext userContext,
            IMapper mapper,
            IVocabDeckRepository deckRepo,
            IVocabDeckWordRepository deckWordRepo,
            IVocabWordRepository wordRepo,
            IVocabStudyProgressRepository progressRepo) : base(logger, userContext, mapper)
        {
            _deckRepo = deckRepo;
            _deckWordRepo = deckWordRepo;
            _wordRepo = wordRepo;
            _progressRepo = progressRepo;
        }

        public async Task<Result<List<StudyCardModel>>> GetDueCardsAsync(string deckId, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error<List<StudyCardModel>>(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var deck = await _deckRepo.FirstOrDefaultAsync(
                    d => d.Id == deckId && d.OwnerId == userId && !d.IsDeleted, ct);
                if (deck == null)
                    return Result.Error<List<StudyCardModel>>(Result.DATA_NOT_EXISTED.Code, "Không tìm thấy deck.");

                var words = await _deckWordRepo.Query(true)
                    .Where(dw => dw.DeckId == deckId)
                    .OrderBy(dw => dw.SortOrder)
                    .Join(_wordRepo.Query(true).Where(w => !w.IsDeleted),
                        dw => dw.WordId, w => w.Id, (dw, w) => w)
                    .ToListAsync(ct);

                if (!words.Any())
                    return Result.Ok(new List<StudyCardModel>());

                var wordIds = words.Select(w => w.Id).ToList();
                var today = DateTime.UtcNow.Date;

                var progresses = await _progressRepo.Query(true)
                    .Where(p => p.UserId == userId && p.DeckId == deckId && wordIds.Contains(p.WordId))
                    .ToDictionaryAsync(p => p.WordId, ct);

                var dueCards = words
                    .Where(w =>
                    {
                        if (!progresses.TryGetValue(w.Id, out var p)) return true; // new card
                        return p.NextReviewDate.Date <= today;                     // due today
                    })
                    .OrderBy(_ => Guid.NewGuid()) // shuffle
                    .Select(w =>
                    {
                        progresses.TryGetValue(w.Id, out var p);
                        return new StudyCardModel
                        {
                            WordId = w.Id,
                            Word = w.Word,
                            Phonetic = w.Phonetic,
                            PartOfSpeech = w.PartOfSpeech,
                            Definition = w.Definition,
                            ExampleSentence = w.ExampleSentence,
                            Translation = w.Translation,
                            AudioUrl = w.AudioUrl,
                            Repetitions = p?.Repetitions ?? 0,
                            IntervalDays = p?.IntervalDays ?? 0,
                            MasteryLevel = p?.MasteryLevel ?? VocabMasteryLevel.New,
                            IsNew = p == null
                        };
                    })
                    .ToList();

                return Result.Ok(dueCards);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lấy danh sách thẻ học thất bại");
                return Result.Exception<List<StudyCardModel>>("Đã có lỗi xảy ra.", ex);
            }
        }

        public async Task<Result> SubmitReviewAsync(SubmitReviewRequest request, CancellationToken ct = default)
        {
            try
            {
                var userId = _userContext.GetUserId();
                if (userId.IsEmpty())
                    return Result.Error(Result.DATA_INVALID.Code, "Không xác định được người dùng.");

                var progress = await _progressRepo.FirstOrDefaultAsync(
                    p => p.UserId == userId && p.WordId == request.WordId && p.DeckId == request.DeckId, ct);

                var (reps, ef, interval, nextDate) = Sm2Algorithm.Calculate(
                    progress?.Repetitions ?? 0,
                    progress?.EasinessFactor ?? 2.5,
                    progress?.IntervalDays ?? 0,
                    request.Quality);

                if (progress == null)
                {
                    progress = new VocabStudyProgressEntity
                    {
                        UserId = userId!,
                        WordId = request.WordId,
                        DeckId = request.DeckId,
                        EasinessFactor = 2.5
                    };
                    progress.Repetitions = reps;
                    progress.EasinessFactor = ef;
                    progress.IntervalDays = interval;
                    progress.NextReviewDate = nextDate;
                    progress.LastReviewDate = DateTime.UtcNow;
                    progress.LastQuality = request.Quality;
                    progress.TotalReviews = 1;
                    progress.CorrectCount = request.Quality >= 3 ? 1 : 0;
                    progress.StreakCount = request.Quality >= 3 ? 1 : 0;
                    progress.MasteryLevel = Sm2Algorithm.GetMasteryLevel(reps, interval);
                    await _progressRepo.InsertAsync(progress, ct);
                }
                else
                {
                    progress.Repetitions = reps;
                    progress.EasinessFactor = ef;
                    progress.IntervalDays = interval;
                    progress.NextReviewDate = nextDate;
                    progress.LastReviewDate = DateTime.UtcNow;
                    progress.LastQuality = request.Quality;
                    progress.TotalReviews++;
                    if (request.Quality >= 3)
                    {
                        progress.CorrectCount++;
                        progress.StreakCount++;
                    }
                    else
                    {
                        progress.StreakCount = 0;
                    }
                    progress.MasteryLevel = Sm2Algorithm.GetMasteryLevel(reps, interval);
                    await _progressRepo.UpdateAsync(progress, ct);
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lưu kết quả ôn tập thất bại");
                return Result.Exception("Đã có lỗi xảy ra.", ex);
            }
        }
    }
}
