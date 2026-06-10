using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empsys.Core.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionDeArticulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Articulos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FamiliaId",
                table: "Articulos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_CategoriaId",
                table: "Articulos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_FamiliaId",
                table: "Articulos",
                column: "FamiliaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_Categorias_CategoriaId",
                table: "Articulos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_Familias_FamiliaId",
                table: "Articulos",
                column: "FamiliaId",
                principalTable: "Familias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_Categorias_CategoriaId",
                table: "Articulos");

            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_Familias_FamiliaId",
                table: "Articulos");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_CategoriaId",
                table: "Articulos");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_FamiliaId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "FamiliaId",
                table: "Articulos");
        }
    }
}
