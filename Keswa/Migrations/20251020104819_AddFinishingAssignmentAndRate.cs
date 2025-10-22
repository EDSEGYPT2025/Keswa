using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishingAssignmentAndRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinishingFee",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FinishingAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinishingBatchId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    AssignmentType = table.Column<int>(type: "int", nullable: false),
                    AssignedQuantity = table.Column<int>(type: "int", nullable: false),
                    RemainingQuantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishingAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinishingAssignments_FinishingBatches_FinishingBatchId",
                        column: x => x.FinishingBatchId,
                        principalTable: "FinishingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishingAssignments_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinishingAssignments_FinishingBatchId",
                table: "FinishingAssignments",
                column: "FinishingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishingAssignments_WorkerId",
                table: "FinishingAssignments",
                column: "WorkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinishingAssignments");

            migrationBuilder.DropColumn(
                name: "FinishingFee",
                table: "Products");
        }
    }
}
