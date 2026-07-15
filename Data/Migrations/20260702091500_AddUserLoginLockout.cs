using ManageLife.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260702091500_AddUserLoginLockout")]
    public partial class AddUserLoginLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            SET @sql = IF(
                (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                 WHERE TABLE_SCHEMA=DATABASE()
                   AND TABLE_NAME='Users'
                   AND COLUMN_NAME='AccessFailedCount') = 0,
                'ALTER TABLE `Users` ADD `AccessFailedCount` int NOT NULL DEFAULT 0',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
            SET @sql = IF(
                (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                 WHERE TABLE_SCHEMA=DATABASE()
                   AND TABLE_NAME='Users'
                   AND COLUMN_NAME='LockoutEnd') = 0,
                'ALTER TABLE `Users` ADD `LockoutEnd` datetime(6) NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `Users` DROP COLUMN IF EXISTS `AccessFailedCount`");
            migrationBuilder.Sql("ALTER TABLE `Users` DROP COLUMN IF EXISTS `LockoutEnd`");
        }
    }
}
