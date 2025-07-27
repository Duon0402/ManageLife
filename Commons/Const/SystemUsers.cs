namespace ManageLife.Commons
{
    public class SystemUsers
    {
        /// <summary>
        /// Đại diện cho các bản ghi do hệ thống tạo (seed data, migration)
        /// </summary>
        public const string System = "System";

        /// <summary>
        /// Đại diện cho người dùng không xác định (khi không có JWT hoặc thông tin người dùng)
        /// </summary>
        public const string Unknown = "Unknown";
    }
}
