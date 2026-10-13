namespace ManageLife.Models
{
    /// <summary>
    /// Kết quả đồng bộ: các thao tác của app đã xử lý tới đâu, cùng thay đổi danh sách/công việc (kèm checklist) từ
    /// mốc app gửi lên và id những cái đã xoá. Không có mốc (hoặc có thao tác bị từ chối) thì trả toàn bộ.
    /// </summary>
    public class TodoSyncModel
    {
        /// <summary>Số thao tác đầu đã xử lý (gồm cả bị từ chối): app bỏ chừng ấy thao tác khỏi hàng chờ.</summary>
        public int Processed { get; set; }

        /// <summary>false: dừng giữa chừng vì lỗi server, thao tác còn lại gửi lại sau; không kèm thay đổi.</summary>
        public bool Complete { get; set; }

        /// <summary>Lý do các thao tác bị từ chối (đã bỏ qua, không gửi lại).</summary>
        public List<string> Rejected { get; set; } = new();

        /// <summary>true: là toàn bộ dữ liệu (app thay hết dữ liệu trên máy), false: chỉ thay đổi từ mốc.</summary>
        public bool Full { get; set; }

        /// <summary>Giờ server (UTC) lúc lấy dữ liệu: app gửi lại làm mốc cho lần đồng bộ sau.</summary>
        public DateTime ServerTime { get; set; }

        public List<TodoListModel> Lists { get; set; } = new();

        /// <summary>Gồm cả công việc đã xong; mỗi công việc kèm checklist đầy đủ.</summary>
        public List<TodoTaskDetailModel> Tasks { get; set; } = new();

        public List<string> DeletedListIds { get; set; } = new();

        public List<string> DeletedTaskIds { get; set; } = new();
    }
}
