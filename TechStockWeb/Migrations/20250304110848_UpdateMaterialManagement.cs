using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStockWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaterialManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MaterialManagement_ProductId",
                table: "MaterialManagement",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialManagement_StateId",
                table: "MaterialManagement",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialManagement_UserId",
                table: "MaterialManagement",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialManagement_Product_ProductId",
                table: "MaterialManagement",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialManagement_States_StateId",
                table: "MaterialManagement",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialManagement_Users_UserId",
                table: "MaterialManagement",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialManagement_Product_ProductId",
                table: "MaterialManagement");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialManagement_States_StateId",
                table: "MaterialManagement");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialManagement_Users_UserId",
                table: "MaterialManagement");

            migrationBuilder.DropIndex(
                name: "IX_MaterialManagement_ProductId",
                table: "MaterialManagement");

            migrationBuilder.DropIndex(
                name: "IX_MaterialManagement_StateId",
                table: "MaterialManagement");

            migrationBuilder.DropIndex(
                name: "IX_MaterialManagement_UserId",
                table: "MaterialManagement");
        }
    }
}
