using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empsys.Core.Migrations
{
    /// <inheritdoc />
    public partial class TablaPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroContrato = table.Column<int>(type: "INTEGER", nullable: false),
                    ContratoNumeroContrato = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Contratos_ContratoNumeroContrato",
                        column: x => x.ContratoNumeroContrato,
                        principalTable: "Contratos",
                        principalColumn: "NumeroContrato",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ContratoNumeroContrato",
                table: "Pagos",
                column: "ContratoNumeroContrato");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UsuarioId",
                table: "Pagos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagos");
        }
    }
}
