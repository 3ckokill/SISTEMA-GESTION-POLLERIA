using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolleriaLovely.Migrations
{
    /// <inheritdoc />
    public partial class RecetasProductoInsumo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductoInsumos",
                columns: table => new
                {
                    IdProductoInsumo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    IdInsumo = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoInsumos", x => x.IdProductoInsumo);
                    table.ForeignKey(
                        name: "FK_ProductoInsumos_Insumos_IdInsumo",
                        column: x => x.IdInsumo,
                        principalTable: "Insumos",
                        principalColumn: "IdInsumo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductoInsumos_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoInsumos_IdInsumo",
                table: "ProductoInsumos",
                column: "IdInsumo");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoInsumos_IdProducto_IdInsumo",
                table: "ProductoInsumos",
                columns: new[] { "IdProducto", "IdInsumo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductoInsumos");
        }
    }
}
