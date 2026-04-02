using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class MarcaConfiguration : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> builder)
    {
        builder.ToTable("Marca");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.NombreMarca)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnName("nombre_marca");

        builder.Property(m => m.Descripcion)
            .HasColumnType("text");

        builder.HasMany(m => m.Productos)
            .WithOne(p => p.Marca)
            .HasForeignKey(p => p.IdMarca)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
