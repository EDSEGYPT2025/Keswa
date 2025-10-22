using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishingDepartmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinishingBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinishingBatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SewingBatchId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishingBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinishingBatches_SewingBatches_SewingBatchId",
                        column: x => x.SewingBatchId,
                        principalTable: "SewingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishingBatches_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinishingBatches_SewingBatchId",
                table: "FinishingBatches",
                column: "SewingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishingBatches_WorkOrderId",
                table: "FinishingBatches",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinishingBatches");
        }
    }
}
