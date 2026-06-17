using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingGroupDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='Settings'
       AND COLUMN_NAME='Group') = 0,
    'ALTER TABLE `Settings` ADD `Group` longtext CHARACTER SET utf8mb4 NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @sql = IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA=DATABASE()
       AND TABLE_NAME='Settings'
       AND COLUMN_NAME='Description') = 0,
    'ALTER TABLE `Settings` ADD `Description` longtext CHARACTER SET utf8mb4 NULL',
    'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `Settings` DROP COLUMN IF EXISTS `Group`");
            migrationBuilder.Sql("ALTER TABLE `Settings` DROP COLUMN IF EXISTS `Description`");
        }
    }
}
