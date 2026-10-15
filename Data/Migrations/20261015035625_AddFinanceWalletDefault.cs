using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceWalletDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "FinanceWallets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "FinanceWallets");
        }
    }
}
