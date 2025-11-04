using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class StageNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentTransfers");

            migrationBuilder.DropTable(
                name: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "CurrentDepartment",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "QualityBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualityBatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinishingBatchId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityBatches_FinishingBatches_FinishingBatchId",
                        column: x => x.FinishingBatchId,
                        principalTable: "FinishingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityBatches_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualityBatchId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    AssignedQuantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantityGradeA = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantityGradeB = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityAssignments_QualityBatches_QualityBatchId",
                        column: x => x.QualityBatchId,
                        principalTable: "QualityBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityAssignments_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssignments_QualityBatchId",
                table: "QualityAssignments",
                column: "QualityBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssignments_WorkerId",
                table: "QualityAssignments",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityBatches_FinishingBatchId",
                table: "QualityBatches",
                column: "FinishingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityBatches_WorkOrderId",
                table: "QualityBatches",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityAssignments");

            migrationBuilder.DropTable(
                name: "QualityBatches");

            migrationBuilder.AddColumn<string>(
                name: "CurrentDepartment",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DepartmentTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FromDepartment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ToDepartment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentTransfers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentTransfers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignedToId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityGradeA = table.Column<int>(type: "int", nullable: false),
                    QuantityGradeB = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransferredQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityInspections_AspNetUsers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityInspections_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentTransfers_ProductId",
                table: "DepartmentTransfers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentTransfers_UserId",
                table: "DepartmentTransfers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_AssignedToId",
                table: "QualityInspections",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_ProductId",
                table: "QualityInspections",
                column: "ProductId");
        }
    }
}
