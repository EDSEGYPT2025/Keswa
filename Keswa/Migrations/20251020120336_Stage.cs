using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class Stage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Earnings",
                table: "WorkerAssignments");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "WorkerAssignments");

            migrationBuilder.RenameColumn(
                name: "ScrappedQuantity",
                table: "WorkerAssignments",
                newName: "TotalScrapped");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalScrapped",
                table: "WorkerAssignments",
                newName: "ScrappedQuantity");

            migrationBuilder.AddColumn<decimal>(
                name: "Earnings",
                table: "WorkerAssignments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "WorkerAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
