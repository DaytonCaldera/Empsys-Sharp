using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empsys.Core.Migrations
{
    /// <inheritdoc />
    public partial class TablaInventarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "NumeroContrato",
                table: "Articulos");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Contratos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArticuloId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioEstimado = table.Column<decimal>(type: "TEXT", nullable: false),
                    NumeroContrato = table.Column<int>(type: "INTEGER", nullable: false),
                    ContratoNumeroContrato = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventarios_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventarios_Contratos_ContratoNumeroContrato",
                        column: x => x.ContratoNumeroContrato,
                        principalTable: "Contratos",
                        principalColumn: "NumeroContrato",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ArticuloId",
                table: "Inventarios",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ContratoNumeroContrato",
                table: "Inventarios",
                column: "ContratoNumeroContrato");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Contratos");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Articulos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumeroContrato",
                table: "Articulos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
