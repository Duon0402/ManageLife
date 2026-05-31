using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoTaskIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // (ReminderAt, IsReminderSent) — cron job query nhắc nhở chạy mỗi phút
            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND INDEX_NAME = 'IX_TodoTasks_ReminderAt_IsReminderSent');
                SET @sql = IF(@e = 0,
                    'CREATE INDEX `IX_TodoTasks_ReminderAt_IsReminderSent` ON `TodoTasks` (`ReminderAt`, `IsReminderSent`)',
                    'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            // (DueDate, IsDeleted) — daily summary query
            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND INDEX_NAME = 'IX_TodoTasks_DueDate_IsDeleted');
                SET @sql = IF(@e = 0,
                    'CREATE INDEX `IX_TodoTasks_DueDate_IsDeleted` ON `TodoTasks` (`DueDate`, `IsDeleted`)',
                    'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            // (CreatedUser, IsDeleted, Status) — GetTodayTasks filter theo user
            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND INDEX_NAME = 'IX_TodoTasks_CreatedUser_IsDeleted_Status');
                SET @sql = IF(@e = 0,
                    'CREATE INDEX `IX_TodoTasks_CreatedUser_IsDeleted_Status` ON `TodoTasks` (`CreatedUser`(255), `IsDeleted`, `Status`)',
                    'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");

            // (TodoListId, IsDeleted) — filter theo list
            migrationBuilder.Sql(@"
                SET @e = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TodoTasks' AND INDEX_NAME = 'IX_TodoTasks_TodoListId_IsDeleted');
                SET @sql = IF(@e = 0,
                    'CREATE INDEX `IX_TodoTasks_TodoListId_IsDeleted` ON `TodoTasks` (`TodoListId`(255), `IsDeleted`)',
                    'SELECT 1');
                PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_TodoTasks_ReminderAt_IsReminderSent` ON `TodoTasks`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_TodoTasks_DueDate_IsDeleted` ON `TodoTasks`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_TodoTasks_CreatedUser_IsDeleted_Status` ON `TodoTasks`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_TodoTasks_TodoListId_IsDeleted` ON `TodoTasks`;");
        }
    }
}
