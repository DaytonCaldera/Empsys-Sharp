using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empsys.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablaPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MesesPagados",
                table: "Pagos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NuevaFechaVencimiento",
                table: "Pagos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MesesPagados",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "NuevaFechaVencimiento",
                table: "Pagos");
        }
    }
}
