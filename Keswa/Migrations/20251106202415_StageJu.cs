using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class StageJu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemainingQuantity",
                table: "FinishingAssignments",
                newName: "TotalScrapped");

            migrationBuilder.RenameColumn(
                name: "AssignmentType",
                table: "FinishingAssignments",
                newName: "ReceivedQuantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalScrapped",
                table: "FinishingAssignments",
                newName: "RemainingQuantity");

            migrationBuilder.RenameColumn(
                name: "ReceivedQuantity",
                table: "FinishingAssignments",
                newName: "AssignmentType");
        }
    }
}
