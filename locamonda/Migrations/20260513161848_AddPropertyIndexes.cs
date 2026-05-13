using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace locamonda.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Properties_Price",
                table: "Properties",
                column: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Properties_Price",
                table: "Properties");
        }
    }
}
