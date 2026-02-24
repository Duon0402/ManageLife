using ManageLife.Base;
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

        public async Task<Result<FileModel>> SaveTempFileAsync(IFormFile file, string? caption = null)
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
                var id = IdHeper.NewId();
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

        public Task<Result<string>> GetFileUrlByFileIdAsync(string fileId)
        {
            //TODO: Triển khai phần này nếu upload lên tele thì lấy ở tele không thì lấy ở local
            throw new NotImplementedException();
        }

        public async Task<Result> UploadToTelegramAsync(string fileId)
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
    }
}
