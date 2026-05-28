using ManageLife.Core;
using ManageLife.Core.Http;
using ManageLife.Models;

namespace ManageLife.ApiClients
{
    public class FreeDictionaryApiClient : BaseHttpApiClient
    {
        public FreeDictionaryApiClient(HttpClient http) : base(http) { }

        public Task<Result<List<FreeDictionaryResponse>>> LookupAsync(string word, CancellationToken ct = default)
            => GetAsync<List<FreeDictionaryResponse>>($"/api/v2/entries/en/{Uri.EscapeDataString(word)}", ct);
    }
}
