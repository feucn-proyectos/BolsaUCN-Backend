using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Publications_BuySellId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "PublicationId",
                table: "Images");

            migrationBuilder.AlterColumn<int>(
                name: "BuySellId",
                table: "Images",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Publications_BuySellId",
                table: "Images",
                column: "BuySellId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Publications_BuySellId",
                table: "Images");

            migrationBuilder.AlterColumn<int>(
                name: "BuySellId",
                table: "Images",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "PublicationId",
                table: "Images",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Publications_BuySellId",
                table: "Images",
                column: "BuySellId",
                principalTable: "Publications",
                principalColumn: "Id");
        }
    }
}
