using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewReviewEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InitialReviewJobId",
                table: "Publications",
                newName: "InitializeReviewsJobId");

            migrationBuilder.AddColumn<string>(
                name: "CloseReviewsJobId",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NewReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<int>(type: "integer", nullable: false),
                    OfferorId = table.Column<int>(type: "integer", nullable: false),
                    ApplicantId = table.Column<int>(type: "integer", nullable: false),
                    OfferorRatingOfApplicant = table.Column<int>(type: "integer", nullable: true),
                    OfferorCommentForApplicant = table.Column<string>(type: "text", nullable: true),
                    ApplicantRatingOfOfferor = table.Column<int>(type: "integer", nullable: true),
                    ApplicantCommentForOfferor = table.Column<string>(type: "text", nullable: true),
                    IsOnTime = table.Column<bool>(type: "boolean", nullable: false),
                    IsPresentable = table.Column<bool>(type: "boolean", nullable: false),
                    IsRespectful = table.Column<bool>(type: "boolean", nullable: false),
                    IsOfferorCommentForApplicantHidden = table.Column<bool>(type: "boolean", nullable: false),
                    IsApplicantCommentForOfferorHidden = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewReviews_AspNetUsers_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewReviews_AspNetUsers_OfferorId",
                        column: x => x.OfferorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewReviews_JobApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewReviews_ApplicantId",
                table: "NewReviews",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_NewReviews_ApplicationId",
                table: "NewReviews",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_NewReviews_OfferorId",
                table: "NewReviews",
                column: "OfferorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewReviews");

            migrationBuilder.DropColumn(
                name: "CloseReviewsJobId",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "InitializeReviewsJobId",
                table: "Publications",
                newName: "InitialReviewJobId");
        }
    }
}
