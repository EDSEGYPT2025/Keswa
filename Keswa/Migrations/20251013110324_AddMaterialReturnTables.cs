using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialReturnTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialReturnNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnNoteNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<int>(type: "int", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReturnNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialReturnNotes_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialReturnNoteDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialReturnNoteId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    QuantityReturned = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReturnNoteDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialReturnNoteDetails_MaterialReturnNotes_MaterialReturnNoteId",
                        column: x => x.MaterialReturnNoteId,
                        principalTable: "MaterialReturnNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReturnNoteDetails_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReturnNoteDetails_MaterialId",
                table: "MaterialReturnNoteDetails",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReturnNoteDetails_MaterialReturnNoteId",
                table: "MaterialReturnNoteDetails",
                column: "MaterialReturnNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReturnNotes_WorkOrderId",
                table: "MaterialReturnNotes",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialReturnNoteDetails");

            migrationBuilder.DropTable(
                name: "MaterialReturnNotes");
        }
    }
}
