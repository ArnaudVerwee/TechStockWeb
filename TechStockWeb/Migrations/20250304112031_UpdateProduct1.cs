using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStockWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProduct1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_TypeArticle_ArticleId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ArticleId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ArticleId",
                table: "Product");

            migrationBuilder.CreateIndex(
                name: "IX_Product_TypeId",
                table: "Product",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_TypeArticle_TypeId",
                table: "Product",
                column: "TypeId",
                principalTable: "TypeArticle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_TypeArticle_TypeId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_TypeId",
                table: "Product");

            migrationBuilder.AddColumn<int>(
                name: "ArticleId",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ArticleId",
                table: "Product",
                column: "ArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_TypeArticle_ArticleId",
                table: "Product",
                column: "ArticleId",
                principalTable: "TypeArticle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
