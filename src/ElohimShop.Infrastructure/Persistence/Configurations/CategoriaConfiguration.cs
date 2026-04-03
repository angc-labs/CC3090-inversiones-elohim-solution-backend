using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categoria");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.NombreCategoria)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnName("nombre_categoria");

        builder.Property(c => c.Descripcion)
            .HasColumnType("text");

        builder.Property(c => c.FechaCreacion)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(c => c.Productos)
            .WithOne(p => p.Categoria)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
