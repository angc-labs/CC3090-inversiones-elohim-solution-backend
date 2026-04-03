using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Producto");

        builder.HasKey(p => p.IdProducto);

        builder.Property(p => p.IdProducto)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_producto");

        builder.Property(p => p.CodigoProducto)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("codigo_producto");

        builder.Property(p => p.NombreProducto)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("nombre_producto");

        builder.Property(p => p.Descripcion)
            .HasColumnType("text");

        builder.Property(p => p.Precio)
            .IsRequired();

        builder.Property(p => p.StockActual)
            .IsRequired()
            .HasColumnName("stock_actual");

        builder.Property(p => p.IdMarca)
            .HasMaxLength(255)
            .HasColumnName("id_marca");

        builder.Property(p => p.CategoriaId)
            .HasMaxLength(255)
            .HasColumnName("categoria_id");

        builder.Property(p => p.FechaVencimiento)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_vencimiento");

        builder.Property(p => p.ImagenPrincipal)
            .HasColumnType("text")
            .HasColumnName("imagen_principal");

        builder.Property(p => p.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_creacion");

        builder.Property(p => p.FechaActualizacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_actualizacion");

        builder.HasOne(p => p.Marca)
            .WithMany(m => m.Productos)
            .HasForeignKey(p => p.IdMarca)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.DetallesReservacion)
            .WithOne(dr => dr.Producto)
            .HasForeignKey(dr => dr.ProductoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.CodigoProducto)
            .IsUnique();
    }
}
