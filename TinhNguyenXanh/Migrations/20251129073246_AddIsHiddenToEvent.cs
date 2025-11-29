using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinhNguyenXanh.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenToEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HiddenReason",
                table: "Events",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HiddenAt",
                table: "Events",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("IsHidden", "Events");
            migrationBuilder.DropColumn("HiddenReason", "Events");
            migrationBuilder.DropColumn("HiddenAt", "Events");
        }
    }
}
