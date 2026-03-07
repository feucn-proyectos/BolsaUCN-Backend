using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovedReviewNavigationFromApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "InitializeReviewsJobId",
                table: "Publications",
                newName: "FinishWorkAndInitializeReviewsJobId");

            migrationBuilder.RenameColumn(
                name: "CloseReviewsJobId",
                table: "Publications",
                newName: "FinalizeAndCloseReviewsJobId");

            migrationBuilder.RenameColumn(
                name: "IsOfferorCommentForApplicantHidden",
                table: "NewReviews",
                newName: "IsOfferorReviewForApplicantHidden");

            migrationBuilder.RenameColumn(
                name: "IsApplicantCommentForOfferorHidden",
                table: "NewReviews",
                newName: "IsApplicantReviewForOfferorHidden");

            migrationBuilder.AddColumn<string>(
                name: "CloseApplicationsJobId",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDeadline",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "OfferorRatingOfApplicant",
                table: "NewReviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ApplicantRatingOfOfferor",
                table: "NewReviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicantReviewCompletedAt",
                table: "NewReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicantReviewHiddenAt",
                table: "NewReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantReviewHiddenReason",
                table: "NewReviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferorReviewCompletedAt",
                table: "NewReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferorReviewHiddenAt",
                table: "NewReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferorReviewHiddenReason",
                table: "NewReviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewClosedAt",
                table: "NewReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Rating",
                table: "AspNetUsers",
                type: "real",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseApplicationsJobId",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ReviewDeadline",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ApplicantReviewCompletedAt",
                table: "NewReviews");

            migrationBuilder.DropColumn(
                name: "ApplicantReviewHiddenAt",
                table: "NewReviews");

            migrationBuilder.DropColumn(
                name: "ApplicantReviewHiddenReason",
                table: "NewReviews");

            migrationBuilder.DropColumn(
                name: "OfferorReviewCompletedAt",
                table: "NewReviews");

            migrationBuilder.DropColumn(
                name: "OfferorReviewHiddenAt",
                table: "NewReviews");

            migrationBuilder.DropColumn(
                name: "OfferorReviewHiddenReason",
                table: "NewReviews");

            migrationBuilder.DropColumn(
                name: "ReviewClosedAt",
                table: "NewReviews");

            migrationBuilder.RenameColumn(
                name: "FinishWorkAndInitializeReviewsJobId",
                table: "Publications",
                newName: "InitializeReviewsJobId");

            migrationBuilder.RenameColumn(
                name: "FinalizeAndCloseReviewsJobId",
                table: "Publications",
                newName: "CloseReviewsJobId");

            migrationBuilder.RenameColumn(
                name: "IsOfferorReviewForApplicantHidden",
                table: "NewReviews",
                newName: "IsOfferorCommentForApplicantHidden");

            migrationBuilder.RenameColumn(
                name: "IsApplicantReviewForOfferorHidden",
                table: "NewReviews",
                newName: "IsApplicantCommentForOfferorHidden");

            migrationBuilder.AlterColumn<int>(
                name: "OfferorRatingOfApplicant",
                table: "NewReviews",
                type: "integer",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ApplicantRatingOfOfferor",
                table: "NewReviews",
                type: "integer",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "JobApplications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);
        }
    }
}
