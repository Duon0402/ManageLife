using ManageLife.Core;
using ManageLife.Models;

namespace ManageLife.Interfaces
{
    public interface IExceptionItemService
    {
        Task<Result<List<ExceptionItemModel>>> GetListExceptionItemsAsync(GetListExceptionItemsRequest request);
        Task<Result<ExceptionItemModel>> GetExceptionItemByIdAsync(GetExceptionItemByIdRequest request);
        Task<Result> CreateExceptionItemAsync(CreateExceptionItemRequest request);
        Task<Result> UpdateExceptionItemAsync(UpdateExceptionItemRequest request);
        Task<Result> DeleteExceptionItemAsync(DeleteExceptionItemRequest request);
    }
}
