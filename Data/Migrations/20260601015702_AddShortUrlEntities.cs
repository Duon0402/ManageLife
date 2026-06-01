using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddShortUrlEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `ShortUrlClicks` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `ShortUrlId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `IpAddress` longtext CHARACTER SET utf8mb4 NULL,
                `UserAgent` longtext CHARACTER SET utf8mb4 NULL,
                `Referer` longtext CHARACTER SET utf8mb4 NULL,
                `CreatedUser` longtext CHARACTER SET utf8mb4 NOT NULL,
                `CreatedTime` datetime(6) NOT NULL,
                PRIMARY KEY (`Id`)
            ) CHARACTER SET utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `ShortUrls` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `Code` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `OriginalUrl` longtext CHARACTER SET utf8mb4 NOT NULL,
                `Title` longtext CHARACTER SET utf8mb4 NULL,
                `ClickCount` int NOT NULL,
                `ExpireAt` datetime(6) NULL,
                `OwnerId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `CreatedUser` longtext CHARACTER SET utf8mb4 NOT NULL,
                `CreatedTime` datetime(6) NOT NULL,
                `UpdatedUser` longtext CHARACTER SET utf8mb4 NULL,
                `UpdatedTime` datetime(6) NULL,
                `DeletedUser` longtext CHARACTER SET utf8mb4 NULL,
                `DeletedTime` datetime(6) NULL,
                `IsDeleted` tinyint(1) NOT NULL,
                PRIMARY KEY (`Id`)
            ) CHARACTER SET utf8mb4;");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_ShortUrlClicks_ShortUrlId_CreatedTime` ON `ShortUrlClicks`;");
            migrationBuilder.CreateIndex(
                name: "IX_ShortUrlClicks_ShortUrlId_CreatedTime",
                table: "ShortUrlClicks",
                columns: new[] { "ShortUrlId", "CreatedTime" });

            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_ShortUrls_Code` ON `ShortUrls`;");
            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_Code",
                table: "ShortUrls",
                column: "Code",
                unique: true);

            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_ShortUrls_OwnerId_IsDeleted` ON `ShortUrls`;");
            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_OwnerId_IsDeleted",
                table: "ShortUrls",
                columns: new[] { "OwnerId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShortUrlClicks");

            migrationBuilder.DropTable(
                name: "ShortUrls");
        }
    }
}
