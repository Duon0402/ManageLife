using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DebtId",
                table: "FinanceTransactions",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinanceDebts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direction = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PersonName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    WalletId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_FinanceDebts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceDebts_FinanceWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "FinanceWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceTransactions_DebtId",
                table: "FinanceTransactions",
                column: "DebtId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceDebts_OwnerId_IsDeleted",
                table: "FinanceDebts",
                columns: new[] { "OwnerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceDebts_WalletId",
                table: "FinanceDebts",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceTransactions_FinanceDebts_DebtId",
                table: "FinanceTransactions",
                column: "DebtId",
                principalTable: "FinanceDebts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinanceTransactions_FinanceDebts_DebtId",
                table: "FinanceTransactions");

            migrationBuilder.DropTable(
                name: "FinanceDebts");

            migrationBuilder.DropIndex(
                name: "IX_FinanceTransactions_DebtId",
                table: "FinanceTransactions");

            migrationBuilder.DropColumn(
                name: "DebtId",
                table: "FinanceTransactions");
        }
    }
}
