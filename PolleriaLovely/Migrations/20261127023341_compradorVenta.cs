using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolleriaLovely.Migrations
{
    /// <inheritdoc />
    public partial class compradorVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreComprador",
                table: "Ventas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreComprador",
                table: "Ventas");
        }
    }
}
