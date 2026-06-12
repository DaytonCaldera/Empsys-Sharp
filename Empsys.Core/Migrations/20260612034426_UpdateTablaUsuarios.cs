using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empsys.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablaUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Contratos_ContratoNumeroContrato",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_ContratoNumeroContrato",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ContratoNumeroContrato",
                table: "Pagos");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Pagos",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Apellido1",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Apellido2",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_NumeroContrato",
                table: "Pagos",
                column: "NumeroContrato");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Contratos_NumeroContrato",
                table: "Pagos",
                column: "NumeroContrato",
                principalTable: "Contratos",
                principalColumn: "NumeroContrato",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Contratos_NumeroContrato",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_NumeroContrato",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Apellido1",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Apellido2",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Clientes");

            migrationBuilder.AlterColumn<int>(
                name: "Tipo",
                table: "Pagos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ContratoNumeroContrato",
                table: "Pagos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ContratoNumeroContrato",
                table: "Pagos",
                column: "ContratoNumeroContrato");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Contratos_ContratoNumeroContrato",
                table: "Pagos",
                column: "ContratoNumeroContrato",
                principalTable: "Contratos",
                principalColumn: "NumeroContrato",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
