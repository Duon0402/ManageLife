using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyTodoTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IF EXISTS: app tự migrate lúc khởi động, MySQL không rollback DDL — nếu bị ngắt giữa chừng
            // thì lần chạy lại không kẹt ở bảng đã drop. Dữ liệu todo cũ bỏ hẳn (todo được thiết kế lại).
            migrationBuilder.Sql("DROP TABLE IF EXISTS `TodoLists`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `TodoTasks`;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TodoLists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoLists", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TodoTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsReminderSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParentTaskId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Recurrence = table.Column<int>(type: "int", nullable: false),
                    RecurrenceEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReminderAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TodoListId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoTasks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
