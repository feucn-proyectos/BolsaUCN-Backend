using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category_Temp",
                table: "Publications",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Publications""
                SET ""Category_Temp"" = CASE
                    WHEN ""Category"" IS NULL THEN NULL
                    ELSE 0
                END
            ");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "Category_Temp",
                table: "Publications",
                newName: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category_Temp",
                table: "Publications",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Publications""
                SET ""Category_Temp"" = ""Category""::text
            ");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "Category_Temp",
                table: "Publications",
                newName: "Category");
        }
    }
}