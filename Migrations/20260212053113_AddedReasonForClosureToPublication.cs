using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedReasonForClosureToPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdminRejectionReason",
                table: "Publications",
                newName: "RejectedByAdminReason");

            migrationBuilder.AddColumn<string>(
                name: "ClosedByAdminReason",
                table: "Publications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedByAdminReason",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "RejectedByAdminReason",
                table: "Publications",
                newName: "AdminRejectionReason");
        }
    }
}
