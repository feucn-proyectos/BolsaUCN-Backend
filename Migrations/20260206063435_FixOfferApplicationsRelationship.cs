using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixOfferApplicationsRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_Publications_OfferId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_OfferId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "JobApplications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfferId",
                table: "JobApplications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_OfferId",
                table: "JobApplications",
                column: "OfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Publications_OfferId",
                table: "JobApplications",
                column: "OfferId",
                principalTable: "Publications",
                principalColumn: "Id");
        }
    }
}
