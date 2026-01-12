using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackagingBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackagingBatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualityBatchId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagingBatches_QualityBatches_QualityBatchId",
                        column: x => x.QualityBatchId,
                        principalTable: "QualityBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingBatches_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackagingAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackagingBatchId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    AssignedQuantity = table.Column<int>(type: "int", nullable: false),
                    CompletedQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagingAssignments_PackagingBatches_PackagingBatchId",
                        column: x => x.PackagingBatchId,
                        principalTable: "PackagingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingAssignments_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagingAssignments_PackagingBatchId",
                table: "PackagingAssignments",
                column: "PackagingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingAssignments_WorkerId",
                table: "PackagingAssignments",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingBatches_QualityBatchId",
                table: "PackagingBatches",
                column: "QualityBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingBatches_WorkOrderId",
                table: "PackagingBatches",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackagingAssignments");

            migrationBuilder.DropTable(
                name: "PackagingBatches");
        }
    }
}
