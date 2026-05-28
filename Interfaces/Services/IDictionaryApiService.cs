using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IDictionaryApiService
    {
        Task<Result<DictionaryLookupResult>> LookupAsync(string word, CancellationToken ct = default);
    }
}
