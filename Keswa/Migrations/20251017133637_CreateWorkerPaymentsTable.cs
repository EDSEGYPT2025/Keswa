using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class CreateWorkerPaymentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerAssignments_WorkerPayment_WorkerPaymentId",
                table: "WorkerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerPayment_Workers_WorkerId",
                table: "WorkerPayment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkerPayment",
                table: "WorkerPayment");

            migrationBuilder.RenameTable(
                name: "WorkerPayment",
                newName: "WorkerPayments");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerPayment_WorkerId",
                table: "WorkerPayments",
                newName: "IX_WorkerPayments_WorkerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkerPayments",
                table: "WorkerPayments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerAssignments_WorkerPayments_WorkerPaymentId",
                table: "WorkerAssignments",
                column: "WorkerPaymentId",
                principalTable: "WorkerPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerPayments_Workers_WorkerId",
                table: "WorkerPayments",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerAssignments_WorkerPayments_WorkerPaymentId",
                table: "WorkerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerPayments_Workers_WorkerId",
                table: "WorkerPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkerPayments",
                table: "WorkerPayments");

            migrationBuilder.RenameTable(
                name: "WorkerPayments",
                newName: "WorkerPayment");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerPayments_WorkerId",
                table: "WorkerPayment",
                newName: "IX_WorkerPayment_WorkerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkerPayment",
                table: "WorkerPayment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerAssignments_WorkerPayment_WorkerPaymentId",
                table: "WorkerAssignments",
                column: "WorkerPaymentId",
                principalTable: "WorkerPayment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerPayment_Workers_WorkerId",
                table: "WorkerPayment",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
