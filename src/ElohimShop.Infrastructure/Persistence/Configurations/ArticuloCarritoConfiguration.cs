using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class ArticuloCarritoConfiguration : IEntityTypeConfiguration<ArticuloCarrito>
{
    public void Configure(EntityTypeBuilder<ArticuloCarrito> builder)
    {
        builder.ToTable("ArticuloCarrito");

        builder.HasKey(a => a.IdArticulo);

        builder.Property(a => a.IdArticulo)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_articulo");

        builder.Property(a => a.CarritoId)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("carrito_id");

        builder.Property(a => a.ProductoId)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("producto_id");

        builder.Property(a => a.NombreProducto)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("nombre_producto");

        builder.Property(a => a.Cantidad)
            .IsRequired()
            .HasColumnName("cantidad");

        builder.Property(a => a.PrecioUnitario)
            .IsRequired()
            .HasColumnType("numeric")
            .HasColumnName("precio_unitario");

        builder.HasOne(a => a.Carrito)
            .WithMany(c => c.Articulos)
            .HasForeignKey(a => a.CarritoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Producto)
            .WithMany()
            .HasForeignKey(a => a.ProductoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.CarritoId, a.ProductoId })
            .IsUnique();
    }
}