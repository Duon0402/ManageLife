using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='Translations'
       AND INDEX_NAME='IX_Translations_LanguageId_IsDeleted') = 0,
    'ALTER TABLE `Translations` ADD INDEX `IX_Translations_LanguageId_IsDeleted` (`LanguageId`, `IsDeleted`)',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='UserTelegramConnections'
       AND INDEX_NAME='IX_UserTelegramConnections_UserId_IsDeleted') = 0,
    'ALTER TABLE `UserTelegramConnections` ADD INDEX `IX_UserTelegramConnections_UserId_IsDeleted` (`UserId`, `IsDeleted`)',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='Habits'
       AND INDEX_NAME='IX_Habits_OwnerId_IsDeleted') = 0,
    'ALTER TABLE `Habits` ADD INDEX `IX_Habits_OwnerId_IsDeleted` (`OwnerId`, `IsDeleted`)',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='VocabDecks'
       AND INDEX_NAME='IX_VocabDecks_TopicId') = 0,
    'ALTER TABLE `VocabDecks` ADD INDEX `IX_VocabDecks_TopicId` (`TopicId`)',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='VocabDecks'
       AND INDEX_NAME='IX_VocabDecks_TopicId') > 0,
    'ALTER TABLE `VocabDecks` DROP INDEX `IX_VocabDecks_TopicId`',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='Habits'
       AND INDEX_NAME='IX_Habits_OwnerId_IsDeleted') > 0,
    'ALTER TABLE `Habits` DROP INDEX `IX_Habits_OwnerId_IsDeleted`',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='UserTelegramConnections'
       AND INDEX_NAME='IX_UserTelegramConnections_UserId_IsDeleted') > 0,
    'ALTER TABLE `UserTelegramConnections` DROP INDEX `IX_UserTelegramConnections_UserId_IsDeleted`',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='Translations'
       AND INDEX_NAME='IX_Translations_LanguageId_IsDeleted') > 0,
    'ALTER TABLE `Translations` DROP INDEX `IX_Translations_LanguageId_IsDeleted`',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }
    }
}
