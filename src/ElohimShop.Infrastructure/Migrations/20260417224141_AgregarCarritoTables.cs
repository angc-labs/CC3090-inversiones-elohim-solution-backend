using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElohimShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCarritoTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carrito",
                columns: table => new
                {
                    id_carrito = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cliente_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carrito", x => x.id_carrito);
                    table.ForeignKey(
                        name: "FK_Carrito_Usuario_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticuloCarrito",
                columns: table => new
                {
                    id_articulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    carrito_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    producto_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nombre_producto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticuloCarrito", x => x.id_articulo);
                    table.ForeignKey(
                        name: "FK_ArticuloCarrito_Carrito_carrito_id",
                        column: x => x.carrito_id,
                        principalTable: "Carrito",
                        principalColumn: "id_carrito",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticuloCarrito_Producto_producto_id",
                        column: x => x.producto_id,
                        principalTable: "Producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticuloCarrito_carrito_id_producto_id",
                table: "ArticuloCarrito",
                columns: new[] { "carrito_id", "producto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticuloCarrito_producto_id",
                table: "ArticuloCarrito",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_Carrito_cliente_id",
                table: "Carrito",
                column: "cliente_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticuloCarrito");

            migrationBuilder.DropTable(
                name: "Carrito");
        }
    }
}
