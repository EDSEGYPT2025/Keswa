using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialIssuanceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialIssuanceNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuanceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    IssuanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialIssuanceNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialIssuanceNotes_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialIssuanceNotes_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialIssuanceNoteDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialIssuanceNoteId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialIssuanceNoteDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialIssuanceNoteDetails_MaterialIssuanceNotes_MaterialIssuanceNoteId",
                        column: x => x.MaterialIssuanceNoteId,
                        principalTable: "MaterialIssuanceNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialIssuanceNoteDetails_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssuanceNoteDetails_MaterialId",
                table: "MaterialIssuanceNoteDetails",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssuanceNoteDetails_MaterialIssuanceNoteId",
                table: "MaterialIssuanceNoteDetails",
                column: "MaterialIssuanceNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssuanceNotes_WarehouseId",
                table: "MaterialIssuanceNotes",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssuanceNotes_WorkOrderId",
                table: "MaterialIssuanceNotes",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialIssuanceNoteDetails");

            migrationBuilder.DropTable(
                name: "MaterialIssuanceNotes");
        }
    }
}
