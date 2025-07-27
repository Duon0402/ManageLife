using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
	public partial class Add_Role_Entity_And_Seed_Data : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "RoleId",
				table: "Users",
				type: "longtext",
				nullable: false)
				.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.CreateTable(
				name: "Roles",
				columns: table => new
				{
					Id = table.Column<string>(type: "varchar(95)", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					Name = table.Column<string>(type: "longtext", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					Description = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					CreatedUser = table.Column<string>(type: "longtext", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
					UpdatedUser = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
					DeletedUser = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					DeletedTime = table.Column<DateTime>(type: "datetime", nullable: true),
					IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Roles", x => x.Id);
				})
				.Annotation("MySql:CharSet", "utf8mb4");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Roles");

			migrationBuilder.DropColumn(
				name: "RoleId",
				table: "Users");
		}
	}
}
