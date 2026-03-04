using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedBuySellModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuySell_ContactInfo",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "BuySell_Location",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "PublicationDate",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "ApplicationDate",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "statusValidation",
                table: "Publications",
                newName: "StatusValidation");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Publications",
                newName: "IsValidated");

            migrationBuilder.RenameColumn(
                name: "DeadlineDate",
                table: "Publications",
                newName: "ApplicationDeadline");

            migrationBuilder.RenameColumn(
                name: "ContactInfo",
                table: "Publications",
                newName: "AdditionalContactInfo");

            migrationBuilder.RenameColumn(
                name: "Banned",
                table: "AspNetUsers",
                newName: "IsBlocked");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Publications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Availability",
                table: "Publications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "Publications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Publications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "JobApplications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OfferId",
                table: "JobApplications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "JobApplications",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AboutMe",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_OfferId",
                table: "JobApplications",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_UserId",
                table: "JobApplications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_AspNetUsers_UserId",
                table: "JobApplications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Publications_OfferId",
                table: "JobApplications",
                column: "OfferId",
                principalTable: "Publications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_AspNetUsers_UserId",
                table: "JobApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_Publications_OfferId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_OfferId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_UserId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "StatusValidation",
                table: "Publications",
                newName: "statusValidation");

            migrationBuilder.RenameColumn(
                name: "IsValidated",
                table: "Publications",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "ApplicationDeadline",
                table: "Publications",
                newName: "DeadlineDate");

            migrationBuilder.RenameColumn(
                name: "AdditionalContactInfo",
                table: "Publications",
                newName: "ContactInfo");

            migrationBuilder.RenameColumn(
                name: "IsBlocked",
                table: "AspNetUsers",
                newName: "Banned");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Publications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "BuySell_ContactInfo",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuySell_Location",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationDate",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDate",
                table: "JobApplications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "AboutMe",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
