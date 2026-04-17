using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Venta");

        builder.HasKey(v => v.IdVenta);

        builder.Property(v => v.IdVenta)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_venta");

        builder.Property(v => v.ReservacionId)
            .HasMaxLength(255)
            .HasColumnName("reservacion_id");

        builder.Property(v => v.MontoTotal)
            .IsRequired()
            .HasColumnType("numeric")
            .HasColumnName("monto_total");

        builder.Property(v => v.UsuarioCajeroId)
            .HasMaxLength(255)
            .HasColumnName("usuario_cajero_id");

        builder.Property(v => v.FechaVenta)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_venta");

        builder.Property(v => v.TipoComprobante)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("tipo_comprobante");

        builder.Property(v => v.EstadoVenta)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)")
            .HasColumnName("estado_venta");

        builder.HasOne(v => v.Reservacion)
            .WithOne(r => r.Venta)
            .HasForeignKey<Venta>(v => v.ReservacionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(v => v.UsuarioCajero)
            .WithMany(u => u.Ventas)
            .HasForeignKey(v => v.UsuarioCajeroId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(v => v.ReservacionId)
            .IsUnique();
    }
}
