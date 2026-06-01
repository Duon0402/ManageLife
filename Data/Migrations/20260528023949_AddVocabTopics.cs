using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserRefreshTokens",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "UserRefreshTokens",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "LanguageId",
                table: "Translations",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `VocabDecks` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
                `Description` longtext CHARACTER SET utf8mb4 NULL,
                `TopicId` longtext CHARACTER SET utf8mb4 NULL,
                `OwnerId` longtext CHARACTER SET utf8mb4 NOT NULL,
                `TotalCards` int NOT NULL,
                `CreatedUser` longtext CHARACTER SET utf8mb4 NOT NULL,
                `CreatedTime` datetime(6) NOT NULL,
                `UpdatedUser` longtext CHARACTER SET utf8mb4 NULL,
                `UpdatedTime` datetime(6) NULL,
                `DeletedUser` longtext CHARACTER SET utf8mb4 NULL,
                `DeletedTime` datetime(6) NULL,
                `IsDeleted` tinyint(1) NOT NULL,
                PRIMARY KEY (`Id`)
            ) CHARACTER SET utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `VocabDeckWords` (
                `DeckId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `WordId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `SortOrder` int NOT NULL,
                `AddedAt` datetime(6) NOT NULL,
                PRIMARY KEY (`DeckId`, `WordId`)
            ) CHARACTER SET utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `VocabStudyProgress` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `WordId` longtext CHARACTER SET utf8mb4 NOT NULL,
                `DeckId` longtext CHARACTER SET utf8mb4 NULL,
                `Repetitions` int NOT NULL,
                `EasinessFactor` double NOT NULL,
                `IntervalDays` int NOT NULL,
                `NextReviewDate` datetime(6) NOT NULL,
                `LastReviewDate` datetime(6) NULL,
                `LastQuality` int NULL,
                `TotalReviews` int NOT NULL,
                `CorrectCount` int NOT NULL,
                `StreakCount` int NOT NULL,
                `MasteryLevel` int NOT NULL,
                `CreatedUser` longtext CHARACTER SET utf8mb4 NOT NULL,
                `CreatedTime` datetime(6) NOT NULL,
                `UpdatedUser` longtext CHARACTER SET utf8mb4 NULL,
                `UpdatedTime` datetime(6) NULL,
                `DeletedUser` longtext CHARACTER SET utf8mb4 NULL,
                `DeletedTime` datetime(6) NULL,
                `IsDeleted` tinyint(1) NOT NULL,
                PRIMARY KEY (`Id`)
            ) CHARACTER SET utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `VocabStudySessions` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `DeckId` longtext CHARACTER SET utf8mb4 NULL,
                `StudyMode` int NOT NULL,
                `StartedAt` datetime(6) NOT NULL,
                `FinishedAt` datetime(6) NULL,
                `TotalCards` int NOT NULL,
                `CorrectCount` int NOT NULL,
                `WrongCount` int NOT NULL,
                `SkippedCount` int NOT NULL,
                `DurationSeconds` int NOT NULL,
                `CreatedUser` longtext CHARACTER SET utf8mb4 NOT NULL,
                `CreatedTime` datetime(6) NOT NULL,
                PRIMARY KEY (`Id`)
            ) CHARACTER SET utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `VocabTopics` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
                `Description` longtext CHARACTER SET utf8mb4 NULL,
                `Color` longtext CHARACTER SET utf8mb4 NULL,
                `Icon` longtext CHARACTER SET utf8mb4 NULL,
                `OwnerId` longtext CHARACTER SET utf8mb4 NOT NULL,
                `IsPublic` tinyint(1) NOT NULL,
                `CreatedUser` longtext CHARACTER SET utf8mb4 NOT NULL,
                `CreatedTime` datetime(6) NOT NULL,
                `UpdatedUser` longtext CHARACTER SET utf8mb4 NULL,
                `UpdatedTime` datetime(6) NULL,
                `DeletedUser` longtext CHARACTER SET utf8mb4 NULL,
                `DeletedTime` datetime(6) NULL,
                `IsDeleted` tinyint(1) NOT NULL,
                PRIMARY KEY (`Id`)
            ) CHARACTER SET utf8mb4;");

            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS `VocabWords` (
                `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `Word` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                `Phonetic` longtext CHARACTER SET utf8mb4 NULL,
                `PartOfSpeech` longtext CHARACTER SET utf8mb4 NULL,
                `Definition` longtext CHARACTER SET utf8mb4 NULL,
                `ExampleSentence` longtext CHARACTER SET utf8mb4 NULL,
                `Transaltion` longtext CHARACTER SET utf8mb4 NULL,
                `AudioUrl` longtext CHARACTER SET utf8mb4 NULL,
                `ImageUrl` longtext CHARACTER SET utf8mb4 NULL,
                `DictionarySource` int NOT NULL,
                `RawDictionaryData` longtext CHARACTER SET utf8mb4 NULL,
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

            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_UserRefreshTokens_RefreshToken` ON `UserRefreshTokens`;");
            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_RefreshToken",
                table: "UserRefreshTokens",
                column: "RefreshToken");

            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_UserRefreshTokens_UserId_IsRevoked_ExpiryTime` ON `UserRefreshTokens`;");
            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_UserId_IsRevoked_ExpiryTime",
                table: "UserRefreshTokens",
                columns: new[] { "UserId", "IsRevoked", "ExpiryTime" });

            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Translations_LanguageId` ON `Translations`;");
            migrationBuilder.CreateIndex(
                name: "IX_Translations_LanguageId",
                table: "Translations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabStudyProgress_UserId_NextReviewDate",
                table: "VocabStudyProgress",
                columns: new[] { "UserId", "NextReviewDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VocabStudySessions_UserId_StartedAt",
                table: "VocabStudySessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_VocabWords_OwnerId_Word_IsDeleted",
                table: "VocabWords",
                columns: new[] { "OwnerId", "Word", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VocabDecks");

            migrationBuilder.DropTable(
                name: "VocabDeckWords");

            migrationBuilder.DropTable(
                name: "VocabStudyProgress");

            migrationBuilder.DropTable(
                name: "VocabStudySessions");

            migrationBuilder.DropTable(
                name: "VocabTopics");

            migrationBuilder.DropTable(
                name: "VocabWords");

            migrationBuilder.DropIndex(
                name: "IX_UserRefreshTokens_RefreshToken",
                table: "UserRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserRefreshTokens_UserId_IsRevoked_ExpiryTime",
                table: "UserRefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Translations_LanguageId",
                table: "Translations");

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
