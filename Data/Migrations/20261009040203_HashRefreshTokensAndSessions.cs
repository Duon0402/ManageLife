using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Data.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokensAndSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Refresh token giờ lưu dạng hash: token cũ (lưu nguyên văn) không còn dùng được → xoá, mọi người đăng nhập lại 1 lần
            migrationBuilder.Sql("DELETE FROM UserRefreshTokens;");

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "UserRefreshTokens",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAt",
                table: "UserRefreshTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "UserRefreshTokens",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "UserRefreshTokens",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SessionStartedAt",
                table: "UserRefreshTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_UserId_SessionId",
                table: "UserRefreshTokens",
                columns: new[] { "UserId", "SessionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Code cũ tra token nguyên văn, không khớp hash: xoá để không còn dòng "chết"
            migrationBuilder.Sql("DELETE FROM UserRefreshTokens;");

            migrationBuilder.DropIndex(
                name: "IX_UserRefreshTokens_UserId_SessionId",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "IssuedAt",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "SessionStartedAt",
                table: "UserRefreshTokens");
        }
    }
}
