using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TodoLists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                    OwnerId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ListId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DueTime = table.Column<TimeOnly>(type: "time(6)", nullable: true),
                    ReminderAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RepeatFrequency = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    RepeatInterval = table.Column<int>(type: "int", nullable: true),
                    RepeatWeekdays = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    RepeatUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoTasks_TodoLists_ListId",
                        column: x => x.ListId,
                        principalTable: "TodoLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TodoChecklistItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDone = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoChecklistItems_TodoTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TodoTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TodoChecklistItems_TaskId_SortOrder",
                table: "TodoChecklistItems",
                columns: new[] { "TaskId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_OwnerId_IsDeleted_SortOrder",
                table: "TodoLists",
                columns: new[] { "OwnerId", "IsDeleted", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_ListId",
                table: "TodoTasks",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_OwnerId_IsDeleted_CompletedAt_DueDate",
                table: "TodoTasks",
                columns: new[] { "OwnerId", "IsDeleted", "CompletedAt", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoTasks_OwnerId_ListId_IsDeleted_SortOrder",
                table: "TodoTasks",
                columns: new[] { "OwnerId", "ListId", "IsDeleted", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodoChecklistItems");

            migrationBuilder.DropTable(
                name: "TodoTasks");

            migrationBuilder.DropTable(
                name: "TodoLists");
        }
    }
}
