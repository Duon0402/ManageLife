using ManageLife.Core;
using ManageLife.Entities;
using ManageLife.Interfaces;
using ManageLife.Models;
using Telegram.Bot;

namespace ManageLife.Services
{
    public class TelegramFileService : ITelegramFileService
    {
        private readonly IConfiguration _config;
        private readonly string _botToken;
        private readonly string? _chatId;
        private readonly TelegramBotClient _botClient;
        private readonly IFileRepository _repo;
        private readonly IAppLogger<TelegramFileService> _logger;
        private readonly ITelegramUploadQueue _queue;
        private readonly string _tempFolder = "temp";

        public TelegramFileService(IFileRepository repo, IConfiguration config, IAppLogger<TelegramFileService> logger, ITelegramUploadQueue queue)
        {
            _config = config;
            _repo = repo;
            _logger = logger;
            _queue = queue;
            _botToken = _config["TelegramSettings:BotToken"]
                ?? throw new InvalidOperationException("TelegramSettings:BotToken is not configured.");
            _chatId = _config["TelegramSettings:ChatIdFileStorage"]
                ?? throw new InvalidOperationException("TelegramSettings:ChatIdFileStorage is not configured.");
            _botClient = new TelegramBotClient(_botToken);
            Directory.CreateDirectory(_tempFolder);
        }

        public async Task<Result<FileModel>> SaveTempFileAsync(IFormFile file, string? caption = null, CancellationToken ct = default)
        {
            string msg;
            bool b;
            string? tempPath = null;
            try
            {
                if (file == null || file.Length == 0)
                {
                    msg = "File không hợp lệ";
                    _logger.Debug(msg);
                    return Result.Error<FileModel>(Result.DATA_INVALID.Code, msg);
                }
                var id = IdHelper.NewId();
                var extension = Path.GetExtension(file.FileName);

                var model = new FileModel
                {
                    Id = id,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    Extension = extension,
                    Status = UploadStatus.Pending,
                };

                var entity = model.MapTo<FileEntity>();

                // Fast path for files < 5MB: Upload directly from memory
                if (file.Length < 5 * 1024 * 1024)
                {
                    _logger.Info($"Direct memory upload for small file: {file.FileName} ({(file.Length / 1024.0 / 1024.0):F2} MB)");
                    entity.Status = UploadStatus.Uploading;
                    b = await _repo.InsertAsync(entity);
                    if (!b) return Result.Error<FileModel>(Result.DATA_NOT_CREATE.Code, "Could not save initial DB state");

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var input = new Telegram.Bot.Types.InputFileStream(ms, file.FileName);
                    var message = await _botClient.SendDocument(chatId: _chatId!, document: input, caption: caption);
                    var telegramFileId = message.Document?.FileId;
                    if (telegramFileId.IsEmpty()) throw new Exception("Telegram FileId null directly");

                    entity.FileId = telegramFileId;
                    entity.Status = UploadStatus.Completed;
                    await _repo.UpdateAsync(entity);

                    model.Status = UploadStatus.Completed;
                    model.FileId = telegramFileId;
                    _logger.Info($"Memory upload success: {file.FileName}");
                    return Result.Ok(model);
                }

                // Slow path for large files: Save to disk and queue
                tempPath = Path.Combine(_tempFolder, id + extension);
                entity.TempPath = tempPath;
                model.TempPath = tempPath;

                await using var fs = new FileStream(
                    tempPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

                await file.CopyToAsync(fs);
                fs.Close();

                b = await _repo.InsertAsync(entity);
                if (!b)
                {
                    File.Delete(tempPath);
                    msg = $"Không thể lưu DB file: {file.FileName}";
                    _logger.Debug(msg);
                    return Result.Error<FileModel>(Result.DATA_NOT_CREATE.Code, msg);
                }
                await _queue.EnqueueAsync(model.Id);
                return Result.Ok(model);
            }
            catch (Exception ex)
            {
                if (tempPath != null && File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                msg = $"Lỗi save temp file: {file.FileName}";
                _logger.Error(ex, msg);
                return Result.Exception<FileModel>(msg, ex);
            }
        }

        public async Task<Result<string>> GetFileUrlByFileIdAsync(string fileId, CancellationToken ct = default)
        {
            try
            {
                var file = await _botClient.GetFile(fileId);
                if (file == null || string.IsNullOrEmpty(file.FilePath))
                {
                    return Result.Error<string>(Result.DATA_NOT_EXISTED.Code, "Could not get file path from Telegram");
                }

                var url = $"https://api.telegram.org/file/bot{_botToken}/{file.FilePath}";
                return Result.Ok(url);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error getting file URL for Telegram FileId: {fileId}");
                return Result.Exception<string>("Error getting file URL from Telegram", ex);
            }
        }

        public async Task<Result<FileEntity>> GetFileEntityAsync(string fileId, CancellationToken ct = default)
        {
            var entity = await _repo.GetAsync(fileId);
            if (entity == null)
            {
                return Result.Error<FileEntity>(Result.DATA_NOT_EXISTED.Code, "File not found in database");
            }
            return Result.Ok(entity);
        }

        public async Task<Result> UploadToTelegramAsync(string fileId, CancellationToken ct = default)
        {
            string msg;
            FileEntity? entity = null;
            try
            {
                entity = await _repo.GetAsync(fileId);
                if (entity == null)
                {
                    msg = $"File not found: {fileId}";
                    _logger.Warning(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                if (entity.Status != UploadStatus.Pending)
                {
                    msg = $"File not in Pending state: {fileId}";
                    _logger.Warning(msg);
                    return Result.Ok();
                }

                if (!File.Exists(entity.TempPath))
                {
                    msg = $"Temp file not exists: {entity.TempPath}";
                    entity.Status = UploadStatus.Failed;
                    await _repo.UpdateAsync(entity);
                    _logger.Warning(msg);
                    return Result.Error(Result.DATA_NOT_EXISTED.Code, msg);
                }

                entity.Status = UploadStatus.Uploading;
                await _repo.UpdateAsync(entity);
                _logger.Info($"Uploading file: {entity.FileName}");

                await using var stream =
                    new FileStream(
                        entity.TempPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 81920,
                        useAsync: true);

                var input = new Telegram.Bot.Types.InputFileStream(stream, entity.FileName);
                Telegram.Bot.Types.Message message = await _botClient.SendDocument(chatId: _chatId!, document: input);
                var telegramFileId = message.Document?.FileId;
                if (telegramFileId.IsEmpty()) throw new Exception("Telegram FileId null");

                entity.FileId = telegramFileId;
                entity.Status = UploadStatus.Completed;
                await _repo.UpdateAsync(entity);
                File.Delete(entity.TempPath);
                _logger.Info($"Upload success: {entity.FileName}");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                if (entity != null)
                {
                    entity.Status = UploadStatus.Failed;
                    await _repo.UpdateAsync(entity);
                }
                msg = $"Upload fail: {fileId}";
                _logger.Error(ex, msg);
                return Result.Exception(msg, ex);
            }
        }
        public async Task<Result<Stream>> DownloadFileStreamAsync(string telegramFileId, CancellationToken ct = default)
        {
            try
            {
                var file = await _botClient.GetFile(telegramFileId);
                if (file == null || string.IsNullOrEmpty(file.FilePath))
                {
                    return Result.Error<Stream>(Result.DATA_NOT_EXISTED.Code, "Could not get file path from Telegram");
                }

                var ms = new MemoryStream();
                await _botClient.DownloadFile(file.FilePath, ms);
                ms.Position = 0;
                return Result.Ok<Stream>(ms);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error downloading file stream for {telegramFileId}");
                return Result.Exception<Stream>("Error downloading file from Telegram", ex);
            }
        }
    }
}
