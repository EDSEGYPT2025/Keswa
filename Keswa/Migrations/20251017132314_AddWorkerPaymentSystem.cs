using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerPaymentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "WorkerAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WorkerPaymentId",
                table: "WorkerAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkerPayment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkerPayment_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkerAssignments_WorkerPaymentId",
                table: "WorkerAssignments",
                column: "WorkerPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerPayment_WorkerId",
                table: "WorkerPayment",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerAssignments_WorkerPayment_WorkerPaymentId",
                table: "WorkerAssignments",
                column: "WorkerPaymentId",
                principalTable: "WorkerPayment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerAssignments_WorkerPayment_WorkerPaymentId",
                table: "WorkerAssignments");

            migrationBuilder.DropTable(
                name: "WorkerPayment");

            migrationBuilder.DropIndex(
                name: "IX_WorkerAssignments_WorkerPaymentId",
                table: "WorkerAssignments");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "WorkerAssignments");

            migrationBuilder.DropColumn(
                name: "WorkerPaymentId",
                table: "WorkerAssignments");
        }
    }
}
