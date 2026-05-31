using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoTaskReminderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AlterColumn chỉ chạy nếu column vẫn là longtext (idempotent)
            migrationBuilder.Sql(@"
                SET @col_type = (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserRefreshTokens' AND COLUMN_NAME = 'UserId');
                SET @sql = IF(@col_type = 'longtext',
                    'ALTER TABLE `UserRefreshTokens` MODIFY COLUMN `UserId` varchar(255) NOT NULL',
                    'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @col_type = (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserRefreshTokens' AND COLUMN_NAME = 'RefreshToken');
                SET @sql = IF(@col_type = 'longtext',
                    'ALTER TABLE `UserRefreshTokens` MODIFY COLUMN `RefreshToken` varchar(255) NOT NULL',
                    'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @col_type = (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Translations' AND COLUMN_NAME = 'LanguageId');
                SET @sql = IF(@col_type = 'longtext',
                    'ALTER TABLE `Translations` MODIFY COLUMN `LanguageId` varchar(255) NOT NULL',
                    'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            // AddColumn TodoTasks — chỉ ADD nếu column chưa tồn tại
            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND COLUMN_NAME = 'CompletedAt');
                SET @sql = IF(@e = 0, 'ALTER TABLE `TodoTasks` ADD COLUMN `CompletedAt` datetime(6) NULL', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND COLUMN_NAME = 'EstimatedMinutes');
                SET @sql = IF(@e = 0, 'ALTER TABLE `TodoTasks` ADD COLUMN `EstimatedMinutes` int NULL', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND COLUMN_NAME = 'IsReminderSent');
                SET @sql = IF(@e = 0, 'ALTER TABLE `TodoTasks` ADD COLUMN `IsReminderSent` tinyint(1) NOT NULL DEFAULT 0', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND COLUMN_NAME = 'Recurrence');
                SET @sql = IF(@e = 0, 'ALTER TABLE `TodoTasks` ADD COLUMN `Recurrence` int NOT NULL DEFAULT 0', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND COLUMN_NAME = 'RecurrenceEndDate');
                SET @sql = IF(@e = 0, 'ALTER TABLE `TodoTasks` ADD COLUMN `RecurrenceEndDate` datetime(6) NULL', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND COLUMN_NAME = 'ReminderAt');
                SET @sql = IF(@e = 0, 'ALTER TABLE `TodoTasks` ADD COLUMN `ReminderAt` datetime(6) NULL', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            // Index — chỉ tạo nếu chưa tồn tại
            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserRefreshTokens' AND INDEX_NAME = 'IX_UserRefreshTokens_RefreshToken');
                SET @sql = IF(@e = 0, 'CREATE INDEX `IX_UserRefreshTokens_RefreshToken` ON `UserRefreshTokens` (`RefreshToken`)', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserRefreshTokens' AND INDEX_NAME = 'IX_UserRefreshTokens_UserId_IsRevoked_ExpiryTime');
                SET @sql = IF(@e = 0, 'CREATE INDEX `IX_UserRefreshTokens_UserId_IsRevoked_ExpiryTime` ON `UserRefreshTokens` (`UserId`, `IsRevoked`, `ExpiryTime`)', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Translations' AND INDEX_NAME = 'IX_Translations_LanguageId');
                SET @sql = IF(@e = 0, 'CREATE INDEX `IX_Translations_LanguageId` ON `Translations` (`LanguageId`)', 'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRefreshTokens_RefreshToken",
                table: "UserRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserRefreshTokens_UserId_IsRevoked_ExpiryTime",
                table: "UserRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Translations_LanguageId",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "TodoTasks");

            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "TodoTasks");

            migrationBuilder.DropColumn(
                name: "IsReminderSent",
                table: "TodoTasks");

            migrationBuilder.DropColumn(
                name: "Recurrence",
                table: "TodoTasks");

            migrationBuilder.DropColumn(
                name: "RecurrenceEndDate",
                table: "TodoTasks");

            migrationBuilder.DropColumn(
                name: "ReminderAt",
                table: "TodoTasks");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserRefreshTokens",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "UserRefreshTokens",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LanguageId",
                table: "Translations",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
