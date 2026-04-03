using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class DetalleReservacionConfiguration : IEntityTypeConfiguration<DetalleReservacion>
{
    public void Configure(EntityTypeBuilder<DetalleReservacion> builder)
    {
        builder.ToTable("DetalleReservacion");

        builder.HasKey(dr => dr.IdDetails);

        builder.Property(dr => dr.IdDetails)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_details");

        builder.Property(dr => dr.ReservacionId)
            .HasMaxLength(255)
            .HasColumnName("reservacion_id");

        builder.Property(dr => dr.ProductoId)
            .HasMaxLength(255)
            .HasColumnName("producto_id");

        builder.Property(dr => dr.Cantidad)
            .IsRequired()
            .HasColumnName("cantidad");

        builder.Property(dr => dr.PrecioUnitario)
            .IsRequired()
            .HasColumnType("numeric")
            .HasColumnName("precio_unitario");

        builder.Property(dr => dr.Subtotal)
            .IsRequired()
            .HasColumnName("subtotal")
            .HasColumnType("numeric")
            .HasComputedColumnSql("cantidad * precio_unitario", stored: true);

        builder.HasOne(dr => dr.Reservacion)
            .WithMany(r => r.Detalles)
            .HasForeignKey(dr => dr.ReservacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dr => dr.Producto)
            .WithMany(p => p.DetallesReservacion)
            .HasForeignKey(dr => dr.ProductoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
