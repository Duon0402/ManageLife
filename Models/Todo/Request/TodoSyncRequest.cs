using System.Text.Json;

namespace ManageLife.Models
{
    public class TodoSyncRequest
    {
        /// <summary>ServerTime của lần đồng bộ trước; null = lấy toàn bộ.</summary>
        public DateTime? Since { get; set; }

        /// <summary>Thao tác làm trên máy chưa gửi, đúng thứ tự đã làm.</summary>
        public List<TodoSyncOperation> Ops { get; set; } = new();
    }

    /// <summary>
    /// Một thao tác của app. Type: createList, updateList, deleteList, reorderLists, createTask, updateTask, deleteTask,
    /// completeTask, reorderTasks, addChecklist, updateChecklist, deleteChecklist.
    /// </summary>
    public class TodoSyncOperation
    {
        public string Type { get; set; } = null!;
        public string? Id { get; set; }
        /// <summary>addChecklist: công việc chứa mục.</summary>
        public string? TaskId { get; set; }
        /// <summary>completeTask: true = xong, false = bỏ xong.</summary>
        public bool Completed { get; set; }
        /// <summary>reorderLists/reorderTasks: thứ tự mới.</summary>
        public List<string>? Ids { get; set; }
        /// <summary>Dữ liệu tạo/sửa (SaveTodoListRequest, SaveTodoTaskRequest hoặc SaveChecklistItemRequest theo Type).</summary>
        public JsonElement? Body { get; set; }
    }
}
