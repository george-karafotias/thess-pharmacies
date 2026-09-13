using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThessPharmacies.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniquePharmacyDutyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PharmacyDuties_PharmacyId_DutyDate_DutyType",
                table: "PharmacyDuties");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDuties_PharmacyId",
                table: "PharmacyDuties",
                column: "PharmacyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PharmacyDuties_PharmacyId",
                table: "PharmacyDuties");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDuties_PharmacyId_DutyDate_DutyType",
                table: "PharmacyDuties",
                columns: new[] { "PharmacyId", "DutyDate", "DutyType" },
                unique: true);
        }
    }
}
