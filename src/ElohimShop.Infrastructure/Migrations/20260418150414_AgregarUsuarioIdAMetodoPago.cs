using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElohimShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuarioIdAMetodoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "usuario_id",
                table: "MetodoPago",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreProducto",
                table: "DetalleReservacion",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "usuario_id",
                table: "MetodoPago");

            migrationBuilder.DropColumn(
                name: "NombreProducto",
                table: "DetalleReservacion");
        }
    }
}
