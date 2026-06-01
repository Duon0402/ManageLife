using ManageLife.ApiClients;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using System.Text.Json;

namespace ManageLife.Services
{
    public class DictionaryApiService : ServiceBase<DictionaryApiService>, IDictionaryApiService
    {
        private readonly FreeDictionaryApiClient _apiClient;

        public DictionaryApiService(IAppLogger<DictionaryApiService> logger, IUserContext userContext, FreeDictionaryApiClient apiClient) : base(logger, userContext)
        {
            _apiClient = apiClient;
        }

        public async Task<Result<DictionaryLookupResult>> LookupAsync(string word, CancellationToken ct = default)
        {
            try
            {
                word = word.Trim().ToLower();
                if (word.IsEmpty())
                {
                    var msg = "Từ cần tra cứu không được để trống.";
                    _logger.Debug(msg);
                    return Result.Error<DictionaryLookupResult>(Result.DATA_INVALID.Code, msg);
                }

                var apiResult = await _apiClient.LookupAsync(word, ct);
                if (!apiResult.IsOk())
                {
                    var msg = $"Tra cứu từ '{word}' thất bại: {apiResult.Message}";
                    _logger.Debug(msg);
                    return Result.Error<DictionaryLookupResult>(apiResult.Code, msg);
                }

                var entry = apiResult.Data!.FirstOrDefault();
                if (entry == null)
                    return Result.Error<DictionaryLookupResult>(Result.DATA_NOT_EXISTED.Code, $"Không tìm thấy từ '{word}'.");

                var phonetic = entry.Phonetics.FirstOrDefault(p => p.Text.IsNotEmpty())?.Text;

                var audioUrl = entry.Phonetics
                    .FirstOrDefault(p => p.Audio.IsNotEmpty() && p.Audio.EndsWith(".mp3"))?.Audio
                    ?? entry.Phonetics.FirstOrDefault(p => p.Audio.IsNotEmpty())?.Audio;

                var meanings = entry.Meanings
                    .SelectMany(m => m.Definitions.Take(2).Select(d => new DictionaryMeaningResult
                    {
                        PartOfSpeech = m.PartOfSpeech,
                        Definition = d.Definition,
                        ExampleSentence = d.Example
                    }))
                    .ToList();

                var result = new DictionaryLookupResult
                {
                    Word = entry.Word,
                    Phonetic = phonetic,
                    AudioUrl = audioUrl,
                    Meanings = meanings,
                    RawJson = JsonSerializer.Serialize(apiResult.Data)
                };

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                var msg = $"Đã có lỗi xảy ra: {ex.Message}";
                _logger.Error(ex, msg);
                return Result.Exception<DictionaryLookupResult>(msg, ex);
            }
        }
    }
}
