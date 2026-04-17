using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElohimShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nombre_categoria = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marca",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nombre_marca = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetodoPago",
                columns: table => new
                {
                    id_metodo_pago = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nombre_metodo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodoPago", x => x.id_metodo_pago);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    correo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    apellido = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    contrasena = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo_usuario = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    id_producto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    codigo_producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nombre_producto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Precio = table.Column<int>(type: "integer", nullable: false),
                    stock_actual = table.Column<int>(type: "integer", nullable: false),
                    id_marca = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    categoria_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    imagen_principal = table.Column<string>(type: "text", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_Producto_Categoria_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "Categoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Producto_Marca_id_marca",
                        column: x => x.id_marca,
                        principalTable: "Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AdministradorPerfil",
                columns: table => new
                {
                    usuario_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministradorPerfil", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_AdministradorPerfil_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientePerfil",
                columns: table => new
                {
                    usuario_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    tipo_cliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientePerfil", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_ClientePerfil_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consulta",
                columns: table => new
                {
                    id_consulta = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    id_cliente = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    id_usuario = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fecha_consulta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consulta", x => x.id_consulta);
                    table.ForeignKey(
                        name: "FK_Consulta_Usuario_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consulta_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservacion",
                columns: table => new
                {
                    id_reservacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    codigo_reservacion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    cliente_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_renovacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado_renovacion = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    total_renovacion = table.Column<decimal>(type: "numeric", nullable: true),
                    metodo_pago_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Pagado = table.Column<bool>(type: "boolean", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    fecha_limite_retiro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservacion", x => x.id_reservacion);
                    table.ForeignKey(
                        name: "FK_Reservacion_MetodoPago_metodo_pago_id",
                        column: x => x.metodo_pago_id,
                        principalTable: "MetodoPago",
                        principalColumn: "id_metodo_pago",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reservacion_Usuario_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TokenRevocado",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    jti = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expira_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revocado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenRevocado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenRevocado_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleReservacion",
                columns: table => new
                {
                    id_details = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reservacion_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    producto_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric", nullable: false, computedColumnSql: "cantidad * precio_unitario", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleReservacion", x => x.id_details);
                    table.ForeignKey(
                        name: "FK_DetalleReservacion_Producto_producto_id",
                        column: x => x.producto_id,
                        principalTable: "Producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DetalleReservacion_Reservacion_reservacion_id",
                        column: x => x.reservacion_id,
                        principalTable: "Reservacion",
                        principalColumn: "id_reservacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Venta",
                columns: table => new
                {
                    id_venta = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reservacion_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    monto_total = table.Column<decimal>(type: "numeric", nullable: false),
                    usuario_cajero_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_venta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_comprobante = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    estado_venta = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venta", x => x.id_venta);
                    table.ForeignKey(
                        name: "FK_Venta_Reservacion_reservacion_id",
                        column: x => x.reservacion_id,
                        principalTable: "Reservacion",
                        principalColumn: "id_reservacion",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Venta_Usuario_usuario_cajero_id",
                        column: x => x.usuario_cajero_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consulta_id_cliente",
                table: "Consulta",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Consulta_id_usuario",
                table: "Consulta",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleReservacion_producto_id",
                table: "DetalleReservacion",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleReservacion_reservacion_id",
                table: "DetalleReservacion",
                column: "reservacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_categoria_id",
                table: "Producto",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_Producto_codigo_producto",
                table: "Producto",
                column: "codigo_producto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producto_id_marca",
                table: "Producto",
                column: "id_marca");

            migrationBuilder.CreateIndex(
                name: "IX_Reservacion_cliente_id",
                table: "Reservacion",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservacion_codigo_reservacion",
                table: "Reservacion",
                column: "codigo_reservacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservacion_metodo_pago_id",
                table: "Reservacion",
                column: "metodo_pago_id");

            migrationBuilder.CreateIndex(
                name: "IX_TokenRevocado_jti",
                table: "TokenRevocado",
                column: "jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenRevocado_usuario_id",
                table: "TokenRevocado",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_correo",
                table: "Usuario",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venta_reservacion_id",
                table: "Venta",
                column: "reservacion_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venta_usuario_cajero_id",
                table: "Venta",
                column: "usuario_cajero_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministradorPerfil");

            migrationBuilder.DropTable(
                name: "ClientePerfil");

            migrationBuilder.DropTable(
                name: "Consulta");

            migrationBuilder.DropTable(
                name: "DetalleReservacion");

            migrationBuilder.DropTable(
                name: "TokenRevocado");

            migrationBuilder.DropTable(
                name: "Venta");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "Reservacion");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Marca");

            migrationBuilder.DropTable(
                name: "MetodoPago");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
