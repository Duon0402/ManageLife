using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramSummarySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sửa lỗi chính tả danh mục tạo sẵn (chỉ đổi khi người dùng chưa tự đổi tên)
            migrationBuilder.Sql("UPDATE FinanceCategories SET Name = 'Mua sắm' WHERE DefaultKey = 'shopping' AND Name = 'Mua sắp'");

            migrationBuilder.AddColumn<bool>(
                name: "DailySummaryEnabled",
                table: "UserTelegramConnections",
                type: "tinyint(1)",
                nullable: false,
                // Người đang liên kết giữ như cũ: vẫn nhận tóm tắt lúc 7:00
                defaultValue: true);

            migrationBuilder.AddColumn<byte>(
                name: "DailySummaryHour",
                table: "UserTelegramConnections",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailySummaryEnabled",
                table: "UserTelegramConnections");

            migrationBuilder.DropColumn(
                name: "DailySummaryHour",
                table: "UserTelegramConnections");
        }
    }
}
