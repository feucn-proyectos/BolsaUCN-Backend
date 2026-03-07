using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FirstRefactorPass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserImages_ProfileBannerId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Individuals");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProfileBannerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "UserImages");

            migrationBuilder.RenameColumn(
                name: "GeneralUserId",
                table: "VerificationCodes",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ProfileBannerId",
                table: "AspNetUsers",
                newName: "Disability");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VerificationCodes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserImages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Publications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "JobApplications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "JobApplications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Images",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserImages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "VerificationCodes",
                newName: "GeneralUserId");

            migrationBuilder.RenameColumn(
                name: "Disability",
                table: "AspNetUsers",
                newName: "ProfileBannerId");

            migrationBuilder.AddColumn<int>(
                name: "ImageType",
                table: "UserImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneralUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SuperAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_AspNetUsers_GeneralUserId",
                        column: x => x.GeneralUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneralUserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    LegalName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_AspNetUsers_GeneralUserId",
                        column: x => x.GeneralUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneralUserId = table.Column<int>(type: "integer", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Individuals_AspNetUsers_GeneralUserId",
                        column: x => x.GeneralUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneralUserId = table.Column<int>(type: "integer", nullable: false),
                    CurriculumVitae = table.Column<string>(type: "text", nullable: false),
                    Disability = table.Column<int>(type: "integer", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    MotivationLetter = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_AspNetUsers_GeneralUserId",
                        column: x => x.GeneralUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProfileBannerId",
                table: "AspNetUsers",
                column: "ProfileBannerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_GeneralUserId",
                table: "Admins",
                column: "GeneralUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_GeneralUserId",
                table: "Companies",
                column: "GeneralUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Individuals_GeneralUserId",
                table: "Individuals",
                column: "GeneralUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_GeneralUserId",
                table: "Students",
                column: "GeneralUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserImages_ProfileBannerId",
                table: "AspNetUsers",
                column: "ProfileBannerId",
                principalTable: "UserImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
