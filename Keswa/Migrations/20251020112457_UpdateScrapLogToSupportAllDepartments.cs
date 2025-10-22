using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScrapLogToSupportAllDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScrapLogs_WorkerAssignments_WorkerAssignmentId",
                table: "ScrapLogs");

            migrationBuilder.RenameColumn(
                name: "WorkerAssignmentId",
                table: "ScrapLogs",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "ScrappedQuantity",
                table: "ScrapLogs",
                newName: "WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ScrapLogs_WorkerAssignmentId",
                table: "ScrapLogs",
                newName: "IX_ScrapLogs_WorkerId");

            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "ScrapLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ScrapLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ScrapLogs_WorkOrderId",
                table: "ScrapLogs",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapLogs_WorkOrders_WorkOrderId",
                table: "ScrapLogs",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapLogs_Workers_WorkerId",
                table: "ScrapLogs",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScrapLogs_WorkOrders_WorkOrderId",
                table: "ScrapLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ScrapLogs_Workers_WorkerId",
                table: "ScrapLogs");

            migrationBuilder.DropIndex(
                name: "IX_ScrapLogs_WorkOrderId",
                table: "ScrapLogs");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "ScrapLogs");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ScrapLogs");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "ScrapLogs",
                newName: "WorkerAssignmentId");

            migrationBuilder.RenameColumn(
                name: "WorkOrderId",
                table: "ScrapLogs",
                newName: "ScrappedQuantity");

            migrationBuilder.RenameIndex(
                name: "IX_ScrapLogs_WorkerId",
                table: "ScrapLogs",
                newName: "IX_ScrapLogs_WorkerAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScrapLogs_WorkerAssignments_WorkerAssignmentId",
                table: "ScrapLogs",
                column: "WorkerAssignmentId",
                principalTable: "WorkerAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
