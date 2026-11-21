using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolleriaLovely.Migrations
{
    /// <inheritdoc />
    public partial class MetodosPagoYCierreCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCaja",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetodoPago",
                table: "Ventas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    IdCaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MontoContado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Diferencia = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.IdCaja);
                    table.ForeignKey(
                        name: "FK_Cajas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdCaja",
                table: "Ventas",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_IdUsuario",
                table: "Cajas",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Cajas_IdCaja",
                table: "Ventas",
                column: "IdCaja",
                principalTable: "Cajas",
                principalColumn: "IdCaja",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Cajas_IdCaja",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_IdCaja",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "IdCaja",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Ventas");
        }
    }
}
