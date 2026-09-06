using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThessPharmacies.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaToDutyImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DutyImports_DutyDate",
                table: "DutyImports");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "DutyImports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DutyImports_Area_DutyDate",
                table: "DutyImports",
                columns: new[] { "Area", "DutyDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DutyImports_Area_DutyDate",
                table: "DutyImports");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "DutyImports");

            migrationBuilder.CreateIndex(
                name: "IX_DutyImports_DutyDate",
                table: "DutyImports",
                column: "DutyDate",
                unique: true);
        }
    }
}
