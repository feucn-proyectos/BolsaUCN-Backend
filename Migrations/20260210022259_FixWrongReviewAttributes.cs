using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixWrongReviewAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Publications");

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "JobApplications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "JobApplications");

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "Publications",
                type: "integer",
                nullable: true);
        }
    }
}
