using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
	public partial class UpdateTransactionEntity_2024_12_29 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "Wallets",
				type: "tinyint(1)",
				nullable: false,
				defaultValue: false,
				oldClrType: typeof(bool),
				oldType: "tinyint(1)",
				oldNullable: true);

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "Transactions",
				type: "tinyint(1)",
				nullable: false,
				defaultValue: false,
				oldClrType: typeof(bool),
				oldType: "tinyint(1)",
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "Wallets",
				type: "tinyint(1)",
				nullable: true,
				oldClrType: typeof(bool),
				oldType: "tinyint(1)");

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "Transactions",
				type: "tinyint(1)",
				nullable: true,
				oldClrType: typeof(bool),
				oldType: "tinyint(1)");
		}
	}
}
