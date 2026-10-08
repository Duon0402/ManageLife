using ManageLife.ApiClients;
using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Core;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Models.CronJob;
using ManageLife.Settings;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ManageLife.Services
{
    public class CronJobService : ServiceBase<CronJobService>, ICronJobService
    {
        private const int MethodPost = 1;

        /// <summary>Job hệ thống: endpoint cron của chính app, gọi bằng POST kèm X-Cron-Secret, giờ VN.</summary>
        private static readonly SystemCronJob[] SystemJobs =
        {
            new("ManageLife · Tóm tắt công việc buổi sáng", "/api/cron/todo-daily-summary", Hour: 7, Minute: 0),
            new("ManageLife · Dọn refresh token hết hạn", "/api/token/cleanup-refresh-tokens", Hour: 3, Minute: 0),
        };

        private static readonly string[] WeekdayNames = { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

        private readonly CronJobApiClient _apiClient;
        private readonly ICacheService _cache;
        private readonly CronJobOptions _options;

        public CronJobService(
            IAppLogger<CronJobService> logger,
            IUserContext userContext,
            CronJobApiClient apiClient,
            ICacheService cache,
            IOptions<CronJobOptions> options) : base(logger, userContext)
        {
            _apiClient = apiClient;
            _cache = cache;
            _options = options.Value;
        }

        public async Task<Result<List<CronJobViewModel>>> GetListAsync(CancellationToken ct = default)
        {
            try
            {
                var jobs = await _cache.TryGetValueAsync<List<CronJobModel>>(CacheSettings.CronJobs());
                if (jobs == null)
                {
                    var rs = await _apiClient.GetJobsAsync(ct);
                    if (!rs.IsOk()) return ApiError<List<CronJobViewModel>>(rs, "Không lấy được danh sách cron job");
                    jobs = rs.Data?.Jobs ?? new();
                    await _cache.SetAsync(jobs, CacheSettings.CronJobs());
                }

                var systemUrls = SystemUrls();
                return Result.Ok(jobs.Select(x => ToViewModel(x, null, systemUrls)).ToList());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy danh sách cron job");
                return Result.Exception<List<CronJobViewModel>>("Có lỗi xảy ra khi lấy danh sách cron job", ex);
            }
        }

        public async Task<Result<CronJobViewModel>> GetByIdAsync(int jobId, CancellationToken ct = default)
        {
            try
            {
                var rs = await _apiClient.GetJobAsync(jobId, ct);
                if (!rs.IsOk() || rs.Data?.JobDetails == null) return ApiError<CronJobViewModel>(rs, "Không tìm thấy cron job");

                var detail = rs.Data.JobDetails;
                return Result.Ok(ToViewModel(detail, detail.ExtendedData, SystemUrls()));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy cron job {jobId}", jobId);
                return Result.Exception<CronJobViewModel>("Có lỗi xảy ra khi lấy cron job", ex);
            }
        }

        public async Task<Result> CreateAsync(SaveCronJobRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var build = BuildWriteModel(request, null);
                if (!build.IsOk()) return build;

                var rs = await _apiClient.CreateJobAsync(build.Data, ct);
                if (!rs.IsOk()) return ApiError(rs, "Tạo cron job thất bại");

                await _cache.RemoveAsync(CacheSettings.CronJobs());
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi tạo cron job");
                return Result.Exception("Có lỗi xảy ra khi tạo cron job", ex);
            }
        }

        public async Task<Result> UpdateAsync(SaveCronJobRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);
                if (request.JobId <= 0) return Result.Error(Result.DATA_INVALID.Code, "Job không hợp lệ");

                //NOTE: Lấy header hiện có để giữ các header khác ngoài secret (UI chỉ quản lý secret)
                var current = await _apiClient.GetJobAsync(request.JobId, ct);
                if (!current.IsOk() || current.Data?.JobDetails == null) return ApiError(current, "Không tìm thấy cron job");

                var build = BuildWriteModel(request, current.Data.JobDetails);
                if (!build.IsOk()) return build;

                var rs = await _apiClient.UpdateJobAsync(request.JobId, build.Data, ct);
                if (!rs.IsOk()) return ApiError(rs, "Cập nhật cron job thất bại");

                await _cache.RemoveAsync(CacheSettings.CronJobs());
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi cập nhật cron job {jobId}", request.JobId);
                return Result.Exception("Có lỗi xảy ra khi cập nhật cron job", ex);
            }
        }

        public async Task<Result> DeleteAsync(CronJobIdRequest request, CancellationToken ct = default)
        {
            try
            {
                var err = Validate(request);
                if (err.IsNotEmpty()) return Result.Error(Result.DATA_INVALID.Code, err);

                var rs = await _apiClient.DeleteJobAsync(request.JobId, ct);
                if (!rs.IsOk()) return ApiError(rs, "Xoá cron job thất bại");

                await _cache.RemoveAsync(CacheSettings.CronJobs());
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi xoá cron job {jobId}", request.JobId);
                return Result.Exception("Có lỗi xảy ra khi xoá cron job", ex);
            }
        }

        public async Task<Result<List<CronJobHistoryModel>>> GetHistoryAsync(int jobId, CancellationToken ct = default)
        {
            try
            {
                var rs = await _apiClient.GetHistoryAsync(jobId, ct);
                if (!rs.IsOk()) return ApiError<List<CronJobHistoryModel>>(rs, "Không lấy được lịch sử chạy");

                var items = (rs.Data?.History ?? new())
                    .OrderByDescending(x => x.Date)
                    .Select(x => new CronJobHistoryModel
                    {
                        Date = FormatUnix(x.Date) ?? "",
                        Duration = x.Duration,
                        Status = x.Status,
                        StatusText = StatusText(x.Status),
                        HttpStatus = x.HttpStatus
                    })
                    .ToList();
                return Result.Ok(items);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi lấy lịch sử cron job {jobId}", jobId);
                return Result.Exception<List<CronJobHistoryModel>>("Có lỗi xảy ra khi lấy lịch sử chạy", ex);
            }
        }

        public async Task<Result<string>> SyncSystemJobsAsync(CancellationToken ct = default)
        {
            try
            {
                var configError = SecretConfigError();
                if (configError != null) return Result.Error<string>(Result.DATA_INVALID.Code, configError);

                //NOTE: Lấy mới (không qua cache) để khớp URL chính xác, tránh tạo trùng
                var list = await _apiClient.GetJobsAsync(ct);
                if (!list.IsOk()) return ApiError<string>(list, "Không lấy được danh sách cron job");
                var jobs = list.Data?.Jobs ?? new();

                int created = 0, updated = 0;
                foreach (var system in SystemJobs)
                {
                    var url = BuildUrl(system.Path);
                    var existing = jobs.FirstOrDefault(x => string.Equals(x.Url, url, StringComparison.OrdinalIgnoreCase));
                    var job = new CronJobWriteModel
                    {
                        Title = system.Title,
                        Url = url,
                        //NOTE: Giữ trạng thái tạm dừng nếu admin đã tắt job; job mới thì bật
                        Enabled = existing?.Enabled ?? true,
                        RequestMethod = MethodPost,
                        Schedule = new CronJobSchedule
                        {
                            Hours = new() { system.Hour },
                            Minutes = new() { system.Minute },
                        },
                        ExtendedData = new CronJobExtendedData
                        {
                            Headers = new() { [CronSecretAttribute.HeaderName] = _options.WebhookSecret },
                            Body = ""
                        }
                    };

                    if (existing != null)
                    {
                        var rs = await _apiClient.UpdateJobAsync(existing.JobId, job, ct);
                        if (!rs.IsOk()) return ApiError<string>(rs, $"Cập nhật job \"{system.Title}\" thất bại");
                        updated++;
                    }
                    else
                    {
                        var rs = await _apiClient.CreateJobAsync(job, ct);
                        if (!rs.IsOk()) return ApiError<string>(rs, $"Tạo job \"{system.Title}\" thất bại");
                        created++;
                    }
                }

                await _cache.RemoveAsync(CacheSettings.CronJobs());
                _logger.Info("Đồng bộ job hệ thống: tạo {created}, cập nhật {updated}", created, updated);
                return Result.Ok($"Đã tạo {created}, cập nhật {updated} job hệ thống");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi đồng bộ job hệ thống");
                return Result.Exception<string>("Có lỗi xảy ra khi đồng bộ job hệ thống", ex);
            }
        }

        #region Helpers

        /// <param name="current">Job hiện tại khi sửa: giữ các thứ form không quản lý (header khác, múi giờ, saveResponses, hạn).</param>
        private Result<CronJobWriteModel> BuildWriteModel(SaveCronJobRequest request, CronJobDetailModel? current)
        {
            var currentHeaders = current?.ExtendedData?.Headers ?? new Dictionary<string, string>();
            var timezone = current?.Schedule?.Timezone.IsNotEmpty() == true ? current.Schedule.Timezone : CronJobSchedule.DefaultTimezone;

            if (!TryParseList(request.Hours, 0, 23, "Giờ", out var hours, out var err)
                || !TryParseList(request.Minutes, 0, 59, "Phút", out var minutes, out err)
                || !TryParseList(request.Mdays, 1, 31, "Ngày", out var mdays, out err)
                || !TryParseList(request.Months, 1, 12, "Tháng", out var months, out err)
                || !TryParseList(request.Wdays, 0, 6, "Thứ", out var wdays, out err))
                return Result.Error<CronJobWriteModel>(Result.DATA_INVALID.Code, err!);

            //NOTE: Secret chỉ do server gắn từ cấu hình, không nhận từ client và không trả ra client.
            // Chỉ gắn cho URL thuộc chính app, tránh người có quyền thêm job gửi secret ra ngoài
            var headers = currentHeaders
                .Where(x => !string.Equals(x.Key, CronSecretAttribute.HeaderName, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Key, x => x.Value);
            if (request.AttachCronSecret)
            {
                var configError = SecretConfigError();
                if (configError != null) return Result.Error<CronJobWriteModel>(Result.DATA_INVALID.Code, configError);
                if (!request.Url.Trim().StartsWith(AppBaseUrl() + "/", StringComparison.OrdinalIgnoreCase))
                    return Result.Error<CronJobWriteModel>(Result.DATA_INVALID.Code, $"Chỉ gắn X-Cron-Secret cho URL của app ({AppBaseUrl()}/...)");
                headers[CronSecretAttribute.HeaderName] = _options.WebhookSecret;
            }

            return Result.Ok(new CronJobWriteModel
            {
                Title = request.Title.Trim(),
                Url = request.Url.Trim(),
                Enabled = request.Enabled,
                SaveResponses = current?.SaveResponses ?? false,
                RequestMethod = request.RequestMethod,
                Schedule = new CronJobSchedule
                {
                    Timezone = timezone,
                    ExpiresAt = current?.Schedule?.ExpiresAt ?? 0,
                    Hours = hours,
                    Minutes = minutes,
                    Mdays = mdays,
                    Months = months,
                    Wdays = wdays
                },
                ExtendedData = new CronJobExtendedData { Headers = headers, Body = request.Body ?? "" }
            });
        }

        /// <summary>"*" hoặc trống = mọi (-1); "1, 15" = danh sách trong [min, max].</summary>
        private static bool TryParseList(string? input, int min, int max, string label, out List<int> values, out string? error)
        {
            values = new() { -1 };
            error = null;
            var text = input?.Trim();
            if (text.IsEmpty() || text == "*") return true;
            if (text!.Length > 200)
            {
                error = $"{label}: danh sách quá dài";
                return false;
            }

            var parsed = new SortedSet<int>();
            foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < min || number > max)
                {
                    error = $"{label}: \"{part}\" không hợp lệ (số từ {min} đến {max}, cách nhau dấu phẩy, * = mọi)";
                    return false;
                }
                parsed.Add(number);
            }

            //NOTE: Chỉ có dấu phẩy (",") thì báo lỗi, không ngầm hiểu là "mọi" (dễ thành chạy mỗi phút)
            if (parsed.Count == 0)
            {
                error = $"{label}: chưa nhập số nào (dùng * nếu muốn mọi {label.ToLowerInvariant()})";
                return false;
            }
            values = parsed.ToList();
            return true;
        }

        private static bool IsAll(List<int>? values) => values == null || values.Count == 0 || values.Contains(-1);

        private static string FormatList(List<int>? values) => IsAll(values) ? "*" : string.Join(",", values!);

        private static string DescribeSchedule(CronJobSchedule? s)
        {
            if (s == null) return "—";
            var singleTime = !IsAll(s.Hours) && s.Hours.Count == 1 && !IsAll(s.Minutes) && s.Minutes.Count == 1;
            var time = singleTime ? $"{s.Hours[0]:00}:{s.Minutes[0]:00}" : null;

            if (time != null && IsAll(s.Mdays) && IsAll(s.Months))
            {
                if (IsAll(s.Wdays)) return $"Hằng ngày {time}";
                return $"{time} " + string.Join(", ", s.Wdays.Where(x => x is >= 0 and <= 6).Select(x => WeekdayNames[x]));
            }
            if (IsAll(s.Hours) && !IsAll(s.Minutes) && IsAll(s.Mdays) && IsAll(s.Months) && IsAll(s.Wdays))
                return $"Mỗi giờ, phút {string.Join(",", s.Minutes)}";

            var parts = new List<string>();
            if (!IsAll(s.Hours)) parts.Add($"giờ {FormatList(s.Hours)}");
            if (!IsAll(s.Minutes)) parts.Add($"phút {FormatList(s.Minutes)}");
            if (!IsAll(s.Mdays)) parts.Add($"ngày {FormatList(s.Mdays)}");
            if (!IsAll(s.Months)) parts.Add($"tháng {FormatList(s.Months)}");
            if (!IsAll(s.Wdays)) parts.Add(string.Join(",", s.Wdays.Where(x => x is >= 0 and <= 6).Select(x => WeekdayNames[x])));
            return parts.Count == 0 ? "Mỗi phút" : string.Join(" · ", parts);
        }

        private static string StatusText(int status) => status switch
        {
            0 => "Chưa chạy",
            1 => "Thành công",
            2 => "Lỗi DNS",
            3 => "Không kết nối được",
            4 => "Lỗi HTTP",
            5 => "Quá thời gian",
            6 => "Phản hồi quá lớn",
            7 => "URL không hợp lệ",
            8 => "Lỗi nội bộ cron-job.org",
            _ => "Không rõ"
        };

        private static string? FormatUnix(long? seconds)
        {
            if (seconds is null or <= 0) return null;
            return DateTimeOffset.FromUnixTimeSeconds(seconds.Value).UtcDateTime.ToVnTimeFromUtc()
                .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        private static CronJobViewModel ToViewModel(CronJobModel job, CronJobExtendedData? extended, HashSet<string> systemUrls)
        {
            var schedule = job.Schedule;
            return new CronJobViewModel
            {
                JobId = job.JobId,
                Title = job.Title,
                Url = job.Url,
                Enabled = job.Enabled,
                RequestMethod = job.RequestMethod,
                Hours = FormatList(schedule?.Hours),
                Minutes = FormatList(schedule?.Minutes),
                Mdays = FormatList(schedule?.Mdays),
                Months = FormatList(schedule?.Months),
                Wdays = FormatList(schedule?.Wdays),
                ScheduleText = DescribeSchedule(schedule),
                Timezone = schedule?.Timezone.IsNotEmpty() == true ? schedule.Timezone : CronJobSchedule.DefaultTimezone,
                Body = extended?.Body,
                AttachCronSecret = extended?.Headers?.Keys.Any(x => string.Equals(x, CronSecretAttribute.HeaderName, StringComparison.OrdinalIgnoreCase)) ?? false,
                IsSystem = systemUrls.Contains(job.Url),
                LastStatus = job.LastStatus,
                LastStatusText = StatusText(job.LastStatus),
                LastExecution = FormatUnix(job.LastExecution),
                NextExecution = FormatUnix(job.NextExecution)
            };
        }

        private string AppBaseUrl() => (_options.AppBaseUrl ?? "").Trim().TrimEnd('/');

        private string? SecretConfigError()
        {
            if (_options.WebhookSecret.IsEmpty()) return "Chưa cấu hình CronJob:WebhookSecret";
            if (!Uri.TryCreate(AppBaseUrl(), UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                return "Chưa cấu hình CronJob:AppBaseUrl (URL gốc của app, vd https://managelife.runasp.net)";
            return null;
        }

        /// <summary>URL các job hệ thống; chưa cấu hình AppBaseUrl thì không đánh dấu được job nào.</summary>
        private HashSet<string> SystemUrls()
            => AppBaseUrl().IsEmpty()
                ? new HashSet<string>()
                : SystemJobs.Select(x => BuildUrl(x.Path)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        private string BuildUrl(string path) => AppBaseUrl() + path;

        //NOTE: API trả 200 nhưng thiếu dữ liệu thì không được giữ mã "00" (sẽ bị hiểu là thành công)
        private Result ApiError(Result rs, string message)
            => Result.Error(rs.IsOk() ? Result.DATA_NOT_EXISTED.Code : rs.Code, ApiErrorMessage(rs, message));

        private Result<T> ApiError<T>(Result rs, string message)
            => Result.Error<T>(rs.IsOk() ? Result.DATA_NOT_EXISTED.Code : rs.Code, ApiErrorMessage(rs, message));

        //NOTE: Kèm lý do từ chối của cron-job.org để admin biết sửa gì; lỗi exception (99) thì không đưa chi tiết ra client.
        // Nội dung được cắt ngắn và che secret/api key phòng khi API trả lại header đã gửi
        private string ApiErrorMessage(Result rs, string message)
        {
            if (rs.IsOk()) return message;
            if (rs.IsException() || rs.ErrorContent.IsEmpty()) return $"{message} ({rs.Message})";

            var content = rs.ErrorContent!;
            if (_options.WebhookSecret.IsNotEmpty()) content = content.Replace(_options.WebhookSecret, "***");
            if (_options.ApiKey.IsNotEmpty()) content = content.Replace(_options.ApiKey, "***");
            //NOTE: Log bản đã che secret (đầy đủ), client chỉ nhận bản cắt ngắn
            _logger.Warning("{message}: {content}", message, content);
            if (content.Length > 300) content = content[..300] + "…";
            return $"{message}: {content}";
        }

        private sealed record SystemCronJob(string Title, string Path, int Hour, int Minute);

        #endregion
    }
}
