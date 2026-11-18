using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolleriaLovely.Migrations
{
    /// <inheritdoc />
    public partial class AsociarPedidoVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdVenta",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdVenta",
                table: "Pedidos",
                column: "IdVenta");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Ventas_IdVenta",
                table: "Pedidos",
                column: "IdVenta",
                principalTable: "Ventas",
                principalColumn: "IdVenta",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Ventas_IdVenta",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_IdVenta",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "IdVenta",
                table: "Pedidos");
        }
    }
}
