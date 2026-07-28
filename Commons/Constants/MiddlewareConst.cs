namespace ManageLife.Commons
{
    public static class MiddlewareConst
    {
        // Các prefix luôn được phép đi qua bất kể middleware chặn toàn app (bảo trì, migrate khẩn cấp)
        // nào đang bật, vì các route này (đăng nhập, admin, api) cần hoạt động ngay cả khi
        // hệ thống đang ở trạng thái hạn chế.
        public static readonly string[] AllowedPathPrefixes =
        [
            "/admin",
            "/auth",
            "/api",
        ];
    }
}
