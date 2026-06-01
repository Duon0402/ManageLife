using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class FixVocabWordTranslationTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @colExists = (SELECT COUNT(*) FROM information_schema.columns
                    WHERE table_schema = DATABASE()
                    AND table_name = 'VocabWords'
                    AND column_name = 'Transaltion');
                SET @sql = IF(@colExists > 0,
                    'ALTER TABLE `VocabWords` RENAME COLUMN `Transaltion` TO `Translation`',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Translation",
                table: "VocabWords",
                newName: "Transaltion");
        }
    }
}
