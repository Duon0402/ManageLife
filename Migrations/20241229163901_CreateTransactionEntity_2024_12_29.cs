using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
	public partial class CreateTransactionEntity_2024_12_29 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "TotalMoney",
				table: "Wallets",
				newName: "Balance");

			migrationBuilder.AlterColumn<string>(
				name: "UpdatedUser",
				table: "Wallets",
				type: "longtext",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "longtext")
				.Annotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.AlterColumn<DateTime>(
				name: "UpdatedTime",
				table: "Wallets",
				type: "datetime",
				nullable: true,
				oldClrType: typeof(DateTime),
				oldType: "datetime");

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "Wallets",
				type: "tinyint(1)",
				nullable: true,
				oldClrType: typeof(bool),
				oldType: "tinyint(1)");

			migrationBuilder.AlterColumn<string>(
				name: "DeletedUser",
				table: "Wallets",
				type: "longtext",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "longtext")
				.Annotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.AlterColumn<DateTime>(
				name: "DeletedTime",
				table: "Wallets",
				type: "datetime",
				nullable: true,
				oldClrType: typeof(DateTime),
				oldType: "datetime");

			migrationBuilder.AlterColumn<DateTime>(
				name: "CreatedTime",
				table: "Wallets",
				type: "datetime",
				nullable: true,
				oldClrType: typeof(DateTime),
				oldType: "datetime");

			migrationBuilder.CreateTable(
				name: "Transactions",
				columns: table => new
				{
					Id = table.Column<string>(type: "varchar(95)", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					TransitionType = table.Column<int>(type: "int", nullable: false),
					TransactionCategoryId = table.Column<string>(type: "longtext", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
					TransactionDate = table.Column<DateTime>(type: "datetime", nullable: false),
					Description = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					CreatedUser = table.Column<string>(type: "longtext", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
					IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: true),
					DeletedUser = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					DeletedTime = table.Column<DateTime>(type: "datetime", nullable: true),
					UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Transactions", x => x.Id);
				})
				.Annotation("MySql:CharSet", "utf8mb4");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Transactions");

			migrationBuilder.RenameColumn(
				name: "Balance",
				table: "Wallets",
				newName: "TotalMoney");

			migrationBuilder.UpdateData(
				table: "Wallets",
				keyColumn: "UpdatedUser",
				keyValue: null,
				column: "UpdatedUser",
				value: "");

			migrationBuilder.AlterColumn<string>(
				name: "UpdatedUser",
				table: "Wallets",
				type: "longtext",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "longtext",
				oldNullable: true)
				.Annotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.AlterColumn<DateTime>(
				name: "UpdatedTime",
				table: "Wallets",
				type: "datetime",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
				oldClrType: typeof(DateTime),
				oldType: "datetime",
				oldNullable: true);

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "Wallets",
				type: "tinyint(1)",
				nullable: false,
				defaultValue: false,
				oldClrType: typeof(bool),
				oldType: "tinyint(1)",
				oldNullable: true);

			migrationBuilder.UpdateData(
				table: "Wallets",
				keyColumn: "DeletedUser",
				keyValue: null,
				column: "DeletedUser",
				value: "");

			migrationBuilder.AlterColumn<string>(
				name: "DeletedUser",
				table: "Wallets",
				type: "longtext",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "longtext",
				oldNullable: true)
				.Annotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.AlterColumn<DateTime>(
				name: "DeletedTime",
				table: "Wallets",
				type: "datetime",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
				oldClrType: typeof(DateTime),
				oldType: "datetime",
				oldNullable: true);

			migrationBuilder.AlterColumn<DateTime>(
				name: "CreatedTime",
				table: "Wallets",
				type: "datetime",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
				oldClrType: typeof(DateTime),
				oldType: "datetime",
				oldNullable: true);
		}
	}
}
