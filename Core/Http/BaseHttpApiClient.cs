using ManageLife.Core;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ManageLife.Core.Http
{
    public abstract class BaseHttpApiClient
    {
        protected readonly HttpClient _http;

        protected BaseHttpApiClient(HttpClient http)
        {
            _http = http;
        }

        protected async Task<Result<T>> GetAsync<T>(string path, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetAsync(path, ct);
                return await ProcessResponseAsync<T>(res);
            }
            catch (Exception ex)
            {
                return Result.Exception<T>($"GET {path} thất bại", ex);
            }
        }

        protected async Task<Result<T>> PostAsync<T>(string path, object? body = null, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.PostAsync(path, body is null ? null : JsonBody(body), ct);
                return await ProcessResponseAsync<T>(res);
            }
            catch (Exception ex)
            {
                return Result.Exception<T>($"POST {path} thất bại", ex);
            }
        }

        protected async Task<Result<T>> PutAsync<T>(string path, object? body = null, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.PutAsync(path, body is null ? null : JsonBody(body), ct);
                return await ProcessResponseAsync<T>(res);
            }
            catch (Exception ex)
            {
                return Result.Exception<T>($"PUT {path} thất bại", ex);
            }
        }

        /// <summary>PATCH không đọc body phản hồi (API có thể trả rỗng).</summary>
        protected async Task<Result> PatchAsync(string path, object body, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.PatchAsync(path, JsonBody(body), ct);
                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    return Result.Error(ErrorCodeFor(res.StatusCode), $"HTTP {(int)res.StatusCode}: {res.ReasonPhrase}", errorContent);
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception($"PATCH {path} thất bại", ex);
            }
        }

        protected async Task<Result> DeleteAsync(string path, CancellationToken ct = default)
        {
            try
            {
                var res = await _http.DeleteAsync(path, ct);
                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    return Result.Error(ErrorCodeFor(res.StatusCode), $"HTTP {(int)res.StatusCode}: {res.ReasonPhrase}", errorContent);
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Exception($"DELETE {path} thất bại", ex);
            }
        }

        private static async Task<Result<T>> ProcessResponseAsync<T>(HttpResponseMessage res)
        {
            if (!res.IsSuccessStatusCode)
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                return Result.Error<T>(ErrorCodeFor(res.StatusCode), $"HTTP {(int)res.StatusCode}: {res.ReasonPhrase}", errorContent);
            }
            var data = await res.Content.ReadFromJsonAsync<T>();
            return Result.Ok(data!);
        }

        //NOTE: Content-Type đúng "application/json" (không kèm "; charset=utf-8" như mặc định của .NET):
        // một số API (vd cron-job.org) so khớp tuyệt đối, khác là bỏ qua body và trả 400. JSON mặc định đã là UTF-8
        private static HttpContent JsonBody(object body)
        {
            var content = JsonContent.Create(body);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

        //NOTE: Dữ liệu gửi lên sai (400/409/422) → DATA_INVALID; còn lại (404, 5xx...) giữ DATA_NOT_EXISTED như trước
        private static string ErrorCodeFor(HttpStatusCode status) => status switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity => Result.DATA_INVALID.Code,
            _ => Result.DATA_NOT_EXISTED.Code
        };
    }
}
