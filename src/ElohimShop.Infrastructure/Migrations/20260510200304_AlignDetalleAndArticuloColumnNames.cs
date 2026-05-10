using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElohimShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignDetalleAndArticuloColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DetalleReservacion: faltaba la columna en migraciones antiguas; el script SQL/manual puede
            // tenerla ya. Solo añadir si no existe.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_attribute a
                        JOIN pg_class c ON c.oid = a.attrelid
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public'
                          AND c.relname = 'DetalleReservacion'
                          AND a.attname = 'nombre_producto'
                          AND a.attnum > 0
                          AND NOT a.attisdropped) THEN
                        ALTER TABLE "DetalleReservacion" ADD COLUMN nombre_producto text NOT NULL DEFAULT '';
                        ALTER TABLE "DetalleReservacion" ALTER COLUMN nombre_producto DROP DEFAULT;
                    END IF;
                END $$;
                """);

            // ArticuloCarrito: migración de carrito creó "Subtotal"; renombrar solo si aún existe.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_attribute a
                        JOIN pg_class c ON c.oid = a.attrelid
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public'
                          AND c.relname = 'ArticuloCarrito'
                          AND a.attname = 'Subtotal'
                          AND a.attnum > 0
                          AND NOT a.attisdropped) THEN
                        ALTER TABLE "ArticuloCarrito" RENAME COLUMN "Subtotal" TO subtotal;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_attribute a
                        JOIN pg_class c ON c.oid = a.attrelid
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public'
                          AND c.relname = 'ArticuloCarrito'
                          AND a.attname = 'subtotal'
                          AND a.attnum > 0
                          AND NOT a.attisdropped)
                      AND NOT EXISTS (
                        SELECT 1
                        FROM pg_attribute a2
                        JOIN pg_class c2 ON c2.oid = a2.attrelid
                        JOIN pg_namespace n2 ON n2.oid = c2.relnamespace
                        WHERE n2.nspname = 'public'
                          AND c2.relname = 'ArticuloCarrito'
                          AND a2.attname = 'Subtotal'
                          AND a2.attnum > 0
                          AND NOT a2.attisdropped) THEN
                        ALTER TABLE "ArticuloCarrito" RENAME COLUMN subtotal TO "Subtotal";
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_attribute a
                        JOIN pg_class c ON c.oid = a.attrelid
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public'
                          AND c.relname = 'DetalleReservacion'
                          AND a.attname = 'nombre_producto'
                          AND a.attnum > 0
                          AND NOT a.attisdropped) THEN
                        ALTER TABLE "DetalleReservacion" DROP COLUMN nombre_producto;
                    END IF;
                END $$;
                """);
        }
    }
}
