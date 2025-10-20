using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberToMaterialRequisition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaterialRequisitionNumber",
                table: "MaterialRequisitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaterialRequisitionId",
                table: "MaterialIssuanceNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssuanceNotes_MaterialRequisitionId",
                table: "MaterialIssuanceNotes",
                column: "MaterialRequisitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssuanceNotes_MaterialRequisitions_MaterialRequisitionId",
                table: "MaterialIssuanceNotes",
                column: "MaterialRequisitionId",
                principalTable: "MaterialRequisitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssuanceNotes_MaterialRequisitions_MaterialRequisitionId",
                table: "MaterialIssuanceNotes");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssuanceNotes_MaterialRequisitionId",
                table: "MaterialIssuanceNotes");

            migrationBuilder.DropColumn(
                name: "MaterialRequisitionNumber",
                table: "MaterialRequisitions");

            migrationBuilder.DropColumn(
                name: "MaterialRequisitionId",
                table: "MaterialIssuanceNotes");
        }
    }
}
