using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramBotCommandEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `TelegramBotCommands` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `Command` longtext CHARACTER SET utf8mb4 NOT NULL,
                `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
                `SortOrder` int NOT NULL,
                `CreatedUser` longtext CHARACTER SET utf8mb4 NOT NULL,
                `CreatedTime` datetime(6) NOT NULL,
                `UpdatedUser` longtext CHARACTER SET utf8mb4 NULL,
                `UpdatedTime` datetime(6) NULL,
                `DeletedUser` longtext CHARACTER SET utf8mb4 NULL,
                `DeletedTime` datetime(6) NULL,
                `IsDeleted` tinyint(1) NOT NULL,
                PRIMARY KEY (`Id`)
            ) CHARACTER SET utf8mb4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramBotCommands");
        }
    }
}
