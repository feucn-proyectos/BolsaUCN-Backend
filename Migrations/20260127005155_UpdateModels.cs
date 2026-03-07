using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Publications",
                newName: "PublicationType");

            migrationBuilder.RenameColumn(
                name: "StatusValidation",
                table: "Publications",
                newName: "ApprovalStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicationType",
                table: "Publications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "ApprovalStatus",
                table: "Publications",
                newName: "StatusValidation");
        }
    }
}
