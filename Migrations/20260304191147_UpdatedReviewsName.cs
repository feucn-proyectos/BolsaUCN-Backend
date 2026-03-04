using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedReviewsName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewReviews_AspNetUsers_ApplicantId",
                table: "NewReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_NewReviews_AspNetUsers_OfferorId",
                table: "NewReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_NewReviews_JobApplications_ApplicationId",
                table: "NewReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewReviews",
                table: "NewReviews");

            migrationBuilder.RenameTable(
                name: "NewReviews",
                newName: "Reviews");

            migrationBuilder.RenameIndex(
                name: "IX_NewReviews_OfferorId",
                table: "Reviews",
                newName: "IX_Reviews_OfferorId");

            migrationBuilder.RenameIndex(
                name: "IX_NewReviews_ApplicationId",
                table: "Reviews",
                newName: "IX_Reviews_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_NewReviews_ApplicantId",
                table: "Reviews",
                newName: "IX_Reviews_ApplicantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reviews",
                table: "Reviews",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_ApplicantId",
                table: "Reviews",
                column: "ApplicantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_OfferorId",
                table: "Reviews",
                column: "OfferorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_JobApplications_ApplicationId",
                table: "Reviews",
                column: "ApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_ApplicantId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_OfferorId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_JobApplications_ApplicationId",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reviews",
                table: "Reviews");

            migrationBuilder.RenameTable(
                name: "Reviews",
                newName: "NewReviews");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_OfferorId",
                table: "NewReviews",
                newName: "IX_NewReviews_OfferorId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ApplicationId",
                table: "NewReviews",
                newName: "IX_NewReviews_ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ApplicantId",
                table: "NewReviews",
                newName: "IX_NewReviews_ApplicantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewReviews",
                table: "NewReviews",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NewReviews_AspNetUsers_ApplicantId",
                table: "NewReviews",
                column: "ApplicantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewReviews_AspNetUsers_OfferorId",
                table: "NewReviews",
                column: "OfferorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewReviews_JobApplications_ApplicationId",
                table: "NewReviews",
                column: "ApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
