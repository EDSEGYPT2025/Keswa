using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddSewingFeeAndEarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SewingRate",
                table: "Products",
                newName: "SewingFee");

            migrationBuilder.AddColumn<decimal>(
                name: "Earnings",
                table: "WorkerAssignments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Earnings",
                table: "WorkerAssignments");

            migrationBuilder.RenameColumn(
                name: "SewingFee",
                table: "Products",
                newName: "SewingRate");
        }
    }
}
