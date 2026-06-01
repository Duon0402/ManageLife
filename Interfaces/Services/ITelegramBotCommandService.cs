using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface ITelegramBotCommandService
    {
        Task<Result<List<TelegramBotCommandModel>>> GetListAsync(CancellationToken ct = default);
        Task<Result> CreateAsync(CreateTelegramBotCommandRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(UpdateTelegramBotCommandRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(DeleteTelegramBotCommandRequest request, CancellationToken ct = default);
    }
}
