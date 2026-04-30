using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManageLife.Migrations
{
    public partial class UpdateChatEntityToBae : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ChatRooms",
                newName: "CreatedTime");

            migrationBuilder.RenameColumn(
                name: "JoinedAt",
                table: "ChatRoomMembers",
                newName: "CreatedTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ChatMessages",
                newName: "CreatedTime");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_RoomId_CreatedAt",
                table: "ChatMessages",
                newName: "IX_ChatMessages_RoomId_CreatedTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "ChatRoomUserStates",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedUser",
                table: "ChatRoomUserStates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "ChatRoomUserStates",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "ChatRoomUserStates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUser",
                table: "ChatRoomUserStates",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedUser",
                table: "ChatRooms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedUser",
                table: "ChatRoomMembers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "ChatRoomMembers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "ChatRoomMembers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUser",
                table: "ChatRoomMembers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedUser",
                table: "ChatMessages",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "ChatRoomUserStates");

            migrationBuilder.DropColumn(
                name: "CreatedUser",
                table: "ChatRoomUserStates");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChatRoomUserStates");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "ChatRoomUserStates");

            migrationBuilder.DropColumn(
                name: "UpdatedUser",
                table: "ChatRoomUserStates");

            migrationBuilder.DropColumn(
                name: "CreatedUser",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "CreatedUser",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedUser",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "CreatedUser",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "CreatedTime",
                table: "ChatRooms",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedTime",
                table: "ChatRoomMembers",
                newName: "JoinedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedTime",
                table: "ChatMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_RoomId_CreatedTime",
                table: "ChatMessages",
                newName: "IX_ChatMessages_RoomId_CreatedAt");
        }
    }
}
