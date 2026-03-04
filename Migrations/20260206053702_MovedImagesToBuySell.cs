using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class MovedImagesToBuySell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Publications_PublicationId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_PublicationId",
                table: "Images");

            migrationBuilder.AddColumn<int>(
                name: "BuySellId",
                table: "Images",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_BuySellId",
                table: "Images",
                column: "BuySellId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Publications_BuySellId",
                table: "Images",
                column: "BuySellId",
                principalTable: "Publications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Publications_BuySellId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_BuySellId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "BuySellId",
                table: "Images");

            migrationBuilder.CreateIndex(
                name: "IX_Images_PublicationId",
                table: "Images",
                column: "PublicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Publications_PublicationId",
                table: "Images",
                column: "PublicationId",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
