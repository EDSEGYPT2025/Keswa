using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishingProductionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinishingProductionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinishingAssignmentId = table.Column<int>(type: "int", nullable: false),
                    QuantityProduced = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishingProductionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinishingProductionLogs_FinishingAssignments_FinishingAssignmentId",
                        column: x => x.FinishingAssignmentId,
                        principalTable: "FinishingAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinishingProductionLogs_FinishingAssignmentId",
                table: "FinishingProductionLogs",
                column: "FinishingAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinishingProductionLogs");
        }
    }
}
