using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empsys.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixTablaInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Contratos_ContratoNumeroContrato",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_ContratoNumeroContrato",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "ContratoNumeroContrato",
                table: "Inventarios");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_NumeroContrato",
                table: "Inventarios",
                column: "NumeroContrato");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Contratos_NumeroContrato",
                table: "Inventarios",
                column: "NumeroContrato",
                principalTable: "Contratos",
                principalColumn: "NumeroContrato",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Contratos_NumeroContrato",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_NumeroContrato",
                table: "Inventarios");

            migrationBuilder.AddColumn<int>(
                name: "ContratoNumeroContrato",
                table: "Inventarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ContratoNumeroContrato",
                table: "Inventarios",
                column: "ContratoNumeroContrato");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Contratos_ContratoNumeroContrato",
                table: "Inventarios",
                column: "ContratoNumeroContrato",
                principalTable: "Contratos",
                principalColumn: "NumeroContrato",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
