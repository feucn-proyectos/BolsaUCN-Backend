using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedJobIdForCreatingInitialReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitialReviewJobId",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkCompletedAt",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkStartedAt",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "InitialReviewJobId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "WorkCompletedAt",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "WorkStartedAt",
                table: "Publications");
        }
    }
}
