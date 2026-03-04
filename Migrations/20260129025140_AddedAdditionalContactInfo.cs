using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedAdditionalContactInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdditionalContactInfo",
                table: "Publications",
                newName: "AdditionalContactPhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalContactEmail",
                table: "Publications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalContactEmail",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "AdditionalContactPhoneNumber",
                table: "Publications",
                newName: "AdditionalContactInfo");
        }
    }
}
