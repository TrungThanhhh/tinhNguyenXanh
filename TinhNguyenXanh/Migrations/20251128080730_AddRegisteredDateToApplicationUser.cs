using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinhNguyenXanh.Migrations
{
    /// <inheritdoc />
    public partial class AddRegisteredDateToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "EventFavorites",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_EventFavorites_ApplicationUserId",
                table: "EventFavorites",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventFavorites_AspNetUsers_ApplicationUserId",
                table: "EventFavorites",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFavorites_AspNetUsers_ApplicationUserId",
                table: "EventFavorites");

            migrationBuilder.DropIndex(
                name: "IX_EventFavorites_ApplicationUserId",
                table: "EventFavorites");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "EventFavorites");

            migrationBuilder.DropColumn(
                name: "RegisteredDate",
                table: "AspNetUsers");
        }
    }
}
