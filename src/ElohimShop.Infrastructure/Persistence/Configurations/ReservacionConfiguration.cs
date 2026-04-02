using ElohimShop.Domain.Entities;
using ElohimShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class ReservacionConfiguration : IEntityTypeConfiguration<Reservacion>
{
    public void Configure(EntityTypeBuilder<Reservacion> builder)
    {
        builder.ToTable("Reservacion");

        builder.HasKey(r => r.IdReservacion);

        builder.Property(r => r.IdReservacion)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_reservacion");

        builder.Property(r => r.CodigoReservacion)
            .IsRequired()
            .HasMaxLength(60)
            .HasColumnName("codigo_reservacion");

        builder.Property(r => r.ClienteId)
            .HasMaxLength(255)
            .HasColumnName("cliente_id");

        builder.Property(r => r.FechaRenovacion)
            .IsRequired()
            .HasColumnType("timestamp")
            .HasColumnName("fecha_renovacion");

        builder.Property(r => r.EstadoRenovacion)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v => string.IsNullOrEmpty(v) ? null : (EstadoRenovacion)Enum.Parse(typeof(EstadoRenovacion), v))
            .HasMaxLength(60)
            .HasColumnType("varchar(60)")
            .HasColumnName("estado_renovacion");

        builder.Property(r => r.TotalRenovacion)
            .HasColumnType("numeric")
            .HasColumnName("total_renovacion");

        builder.Property(r => r.MetodoPagoId)
            .HasMaxLength(255)
            .HasColumnName("metodo_pago_id");

        builder.Property(r => r.Pagado)
            .IsRequired();

        builder.Property(r => r.Observaciones)
            .HasColumnType("text");

        builder.Property(r => r.FechaLimiteRetiro)
            .IsRequired()
            .HasColumnType("timestamp")
            .HasColumnName("fecha_limite_retiro");

        builder.HasOne(r => r.Cliente)
            .WithMany(c => c.Reservaciones)
            .HasForeignKey(r => r.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.MetodoPago)
            .WithMany(mp => mp.Reservaciones)
            .HasForeignKey(r => r.MetodoPagoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Detalles)
            .WithOne(dr => dr.Reservacion)
            .HasForeignKey(dr => dr.ReservacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Venta)
            .WithOne(v => v.Reservacion)
            .HasForeignKey<Venta>(v => v.ReservacionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.CodigoReservacion)
            .IsUnique();
    }
}
