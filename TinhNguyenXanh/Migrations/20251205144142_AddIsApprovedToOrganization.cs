using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinhNguyenXanh.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Organizations");
        }
    }
}
