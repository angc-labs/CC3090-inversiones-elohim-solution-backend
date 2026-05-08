using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElohimShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarStripeReservacionMetodoPagoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "stripe_customer_id",
                table: "Usuario",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_payment_intent_id",
                table: "Reservacion",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alias_tarjeta",
                table: "MetodoPago",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expira_anio",
                table: "MetodoPago",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expira_mes",
                table: "MetodoPago",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marca_tarjeta",
                table: "MetodoPago",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_payment_method_id",
                table: "MetodoPago",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ultimos_digitos",
                table: "MetodoPago",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stripe_customer_id",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "stripe_payment_intent_id",
                table: "Reservacion");

            migrationBuilder.DropColumn(
                name: "alias_tarjeta",
                table: "MetodoPago");

            migrationBuilder.DropColumn(
                name: "expira_anio",
                table: "MetodoPago");

            migrationBuilder.DropColumn(
                name: "expira_mes",
                table: "MetodoPago");

            migrationBuilder.DropColumn(
                name: "marca_tarjeta",
                table: "MetodoPago");

            migrationBuilder.DropColumn(
                name: "stripe_payment_method_id",
                table: "MetodoPago");

            migrationBuilder.DropColumn(
                name: "ultimos_digitos",
                table: "MetodoPago");
        }
    }
}
