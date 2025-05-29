using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStockWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_UserId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Products",
                newName: "AssignedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_UserId",
                table: "Products",
                newName: "IX_Products_AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_AssignedUserId",
                table: "Products",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_AssignedUserId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                table: "Products",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_AssignedUserId",
                table: "Products",
                newName: "IX_Products_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_UserId",
                table: "Products",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
