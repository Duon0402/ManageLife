using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    /// <summary>Danh sách id theo thứ tự mới; SortOrder = vị trí trong mảng.</summary>
    public class ReorderRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "Danh sách id không được để trống")]
        public List<string> Ids { get; set; } = new();
    }
}
