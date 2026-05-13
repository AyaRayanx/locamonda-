using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace locamonda.Migrations
{
    /// <inheritdoc />
    public partial class FixReportsTableStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Status if missing
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            // Add PropertyId if missing
            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "Reports",
                type: "int",
                nullable: true);

            // Create index for PropertyId
            migrationBuilder.CreateIndex(
                name: "IX_Reports_PropertyId",
                table: "Reports",
                column: "PropertyId");

            // Add foreign key for PropertyId
            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Properties_PropertyId",
                table: "Reports",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.SetNull);

            // Drop IsResolved if it exists
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Reports') AND name = 'IsResolved') ALTER TABLE Reports DROP COLUMN IsResolved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
