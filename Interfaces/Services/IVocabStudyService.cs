using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IVocabStudyService
    {
        Task<Result<List<StudyCardModel>>> GetDueCardsAsync(string deckId, CancellationToken ct = default);
        Task<Result> SubmitReviewAsync(SubmitReviewRequest request, CancellationToken ct = default);
    }
}
