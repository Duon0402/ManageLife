using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceRecurring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinanceRecurrings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Kind = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    WalletId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToWalletId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RepeatFrequency = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RepeatInterval = table.Column<int>(type: "int", nullable: false),
                    RepeatWeekdays = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    RepeatMonthDay = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NextDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPaused = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_FinanceRecurrings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceRecurrings_FinanceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "FinanceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceRecurrings_FinanceWallets_ToWalletId",
                        column: x => x.ToWalletId,
                        principalTable: "FinanceWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceRecurrings_FinanceWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "FinanceWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceRecurrings_CategoryId",
                table: "FinanceRecurrings",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceRecurrings_OwnerId_IsDeleted",
                table: "FinanceRecurrings",
                columns: new[] { "OwnerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceRecurrings_ToWalletId",
                table: "FinanceRecurrings",
                column: "ToWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceRecurrings_WalletId",
                table: "FinanceRecurrings",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinanceRecurrings");
        }
    }
}
