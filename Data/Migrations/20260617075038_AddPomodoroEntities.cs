using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddPomodoroEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AlterColumn varchar(255) để hỗ trợ index — idempotent với MODIFY COLUMN
            migrationBuilder.Sql(@"ALTER TABLE `VocabDecks` MODIFY COLUMN `TopicId` varchar(255) CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `UserTelegramConnections` MODIFY COLUMN `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Habits` MODIFY COLUMN `OwnerId` varchar(255) CHARACTER SET utf8mb4 NOT NULL;");

            // Description + Group đã được thêm bởi migration AddSettingGroupDescription — bỏ qua

            migrationBuilder.CreateTable(
                name: "PomodoroSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSessions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PomodoroSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    YoutubeUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackgroundFileId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FocusMinutes = table.Column<int>(type: "int", nullable: false),
                    ShortBreakMinutes = table.Column<int>(type: "int", nullable: false),
                    LongBreakMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSettings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Các index dưới đây có thể đã tồn tại từ AddPerformanceIndexes — dùng idempotent SQL
            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='VocabDecks' AND INDEX_NAME='IX_VocabDecks_TopicId') = 0,
    'ALTER TABLE `VocabDecks` ADD INDEX `IX_VocabDecks_TopicId` (`TopicId`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='UserTelegramConnections' AND INDEX_NAME='IX_UserTelegramConnections_UserId_IsDeleted') = 0,
    'ALTER TABLE `UserTelegramConnections` ADD INDEX `IX_UserTelegramConnections_UserId_IsDeleted` (`UserId`, `IsDeleted`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='Translations' AND INDEX_NAME='IX_Translations_LanguageId_IsDeleted') = 0,
    'ALTER TABLE `Translations` ADD INDEX `IX_Translations_LanguageId_IsDeleted` (`LanguageId`, `IsDeleted`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='Habits' AND INDEX_NAME='IX_Habits_OwnerId_IsDeleted') = 0,
    'ALTER TABLE `Habits` ADD INDEX `IX_Habits_OwnerId_IsDeleted` (`OwnerId`, `IsDeleted`)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_UserId_StartedAt",
                table: "PomodoroSessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSettings_UserId",
                table: "PomodoroSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PomodoroSessions");

            migrationBuilder.DropTable(
                name: "PomodoroSettings");

            migrationBuilder.DropIndex(
                name: "IX_VocabDecks_TopicId",
                table: "VocabDecks");

            migrationBuilder.DropIndex(
                name: "IX_UserTelegramConnections_UserId_IsDeleted",
                table: "UserTelegramConnections");

            migrationBuilder.DropIndex(
                name: "IX_Translations_LanguageId_IsDeleted",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Habits_OwnerId_IsDeleted",
                table: "Habits");

            // Description + Group thuộc về migration AddSettingGroupDescription — không drop ở đây

            migrationBuilder.AlterColumn<string>(
                name: "TopicId",
                table: "VocabDecks",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserTelegramConnections",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Habits",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
