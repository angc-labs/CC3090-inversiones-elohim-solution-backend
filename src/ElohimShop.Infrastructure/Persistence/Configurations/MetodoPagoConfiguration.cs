using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("MetodoPago");

        builder.HasKey(mp => mp.IdMetodoPago);

        builder.Property(mp => mp.IdMetodoPago)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_metodo_pago");

        builder.Property(mp => mp.NombreMetodo)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnName("nombre_metodo");

        builder.Property(mp => mp.Descripcion)
            .HasColumnType("text");

        builder.Property(mp => mp.Activo)
            .IsRequired();

        builder.HasMany(mp => mp.Reservaciones)
            .WithOne(r => r.MetodoPago)
            .HasForeignKey(r => r.MetodoPagoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
