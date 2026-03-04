using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovedCVUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "CVs");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRequestedAt",
                table: "CVs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRequestedAt",
                table: "CVs");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CVs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "CVs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "CVs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
