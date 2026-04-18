using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class CarritoConfiguration : IEntityTypeConfiguration<Carrito>
{
    public void Configure(EntityTypeBuilder<Carrito> builder)
    {
        builder.ToTable("Carrito");

        builder.HasKey(c => c.IdCarrito);

        builder.Property(c => c.IdCarrito)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_carrito");

        builder.Property(c => c.ClienteId)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("cliente_id");

        builder.Property(c => c.Activo)
            .IsRequired()
            .HasColumnName("activo");

        builder.Property(c => c.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_creacion");

        builder.Property(c => c.FechaActualizacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_actualizacion");

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Articulos)
            .WithOne(a => a.Carrito)
            .HasForeignKey(a => a.CarritoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ClienteId)
            .IsUnique();
    }
}