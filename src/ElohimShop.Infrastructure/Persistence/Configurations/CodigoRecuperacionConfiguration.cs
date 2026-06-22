using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class CodigoRecuperacionConfiguration : IEntityTypeConfiguration<CodigoRecuperacion>
{
    public void Configure(EntityTypeBuilder<CodigoRecuperacion> builder)
    {
        builder.ToTable("CodigosRecuperacion");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(c => c.UsuarioId)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(c => c.CodigoHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.Usado)
            .IsRequired();

        builder.Property(c => c.FechaCreacion)
            .IsRequired();

        builder.Property(c => c.FechaExpiracion)
            .IsRequired();

        builder.HasIndex(c => new { c.UsuarioId, c.Usado });
    }
}
