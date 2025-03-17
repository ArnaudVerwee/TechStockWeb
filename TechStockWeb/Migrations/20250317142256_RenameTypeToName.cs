using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStockWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenameTypeToName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "TypeArticle",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TypeArticle",
                newName: "Type");
        }
    }
}
