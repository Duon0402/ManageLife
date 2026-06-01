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
            migrationBuilder.CreateTable(
                name: "ShortUrlClicks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortUrlId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Referer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortUrlClicks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ShortUrls",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClickCount = table.Column<int>(type: "int", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OwnerId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortUrls", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VocabDecks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TopicId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalCards = table.Column<int>(type: "int", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabDecks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VocabDeckWords",
                columns: table => new
                {
                    DeckId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WordId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabDeckWords", x => new { x.DeckId, x.WordId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VocabStudyProgress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WordId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeckId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Repetitions = table.Column<int>(type: "int", nullable: false),
                    EasinessFactor = table.Column<double>(type: "double", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastReviewDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastQuality = table.Column<int>(type: "int", nullable: true),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    StreakCount = table.Column<int>(type: "int", nullable: false),
                    MasteryLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabStudyProgress", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VocabStudySessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeckId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StudyMode = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TotalCards = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    WrongCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabStudySessions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VocabTopics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabTopics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VocabWords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Word = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phonetic = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PartOfSpeech = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Definition = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExampleSentence = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Translation = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AudioUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DictionarySource = table.Column<int>(type: "int", nullable: false),
                    RawDictionaryData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedUser = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedUser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabWords", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrlClicks_ShortUrlId_CreatedTime",
                table: "ShortUrlClicks",
                columns: new[] { "ShortUrlId", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_Code",
                table: "ShortUrls",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_OwnerId_IsDeleted",
                table: "ShortUrls",
                columns: new[] { "OwnerId", "IsDeleted" });

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
                name: "ShortUrlClicks");

            migrationBuilder.DropTable(
                name: "ShortUrls");

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
        }
    }
}
