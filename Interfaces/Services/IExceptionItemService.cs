using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IExceptionItemService
    {
        Task<Result<List<ExceptionItemModel>>> GetListExceptionItemsAsync(GetListExceptionItemsRequest request, CancellationToken ct = default);
        Task<Result<ExceptionItemModel>> GetExceptionItemByIdAsync(GetExceptionItemByIdRequest request, CancellationToken ct = default);
        Task<Result> CreateExceptionItemAsync(CreateExceptionItemRequest request, CancellationToken ct = default);
        Task<Result> UpdateExceptionItemAsync(UpdateExceptionItemRequest request, CancellationToken ct = default);
        Task<Result> DeleteExceptionItemAsync(DeleteExceptionItemRequest request, CancellationToken ct = default);
    }
}
