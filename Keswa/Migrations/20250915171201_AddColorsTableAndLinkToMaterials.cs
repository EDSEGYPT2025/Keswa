using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Keswa.Migrations
{
    /// <inheritdoc />
    public partial class AddColorsTableAndLinkToMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Materials");

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ColorId",
                table: "Materials",
                column: "ColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Colors_ColorId",
                table: "Materials",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Colors_ColorId",
                table: "Materials");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropIndex(
                name: "IX_Materials_ColorId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Materials");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
