using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitCostToProductionReceiptLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "ProductionReceiptLogs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "ProductionReceiptLogs");
        }
    }
}
