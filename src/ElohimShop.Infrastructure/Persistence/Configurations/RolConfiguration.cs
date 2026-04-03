using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Rol");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(r => r.Descripcion)
            .HasColumnType("text");

        builder.Property(r => r.FechaCreacion)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(r => r.Administradores)
            .WithOne(a => a.Rol)
            .HasForeignKey(a => a.IdRol)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
