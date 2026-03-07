using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnnecessaryAttributesInPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "UserAppealJustification",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "OpenSpots",
                table: "Publications",
                newName: "ReviewStatus");

            migrationBuilder.AddColumn<int>(
                name: "AvailableSlots",
                table: "Publications",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSlots",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "ReviewStatus",
                table: "Publications",
                newName: "OpenSpots");

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Publications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserAppealJustification",
                table: "Publications",
                type: "text",
                nullable: true);
        }
    }
}
