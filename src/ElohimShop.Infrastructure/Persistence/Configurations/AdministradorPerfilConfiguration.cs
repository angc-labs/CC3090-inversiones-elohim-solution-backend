using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class AdministradorPerfilConfiguration : IEntityTypeConfiguration<AdministradorPerfil>
{
    public void Configure(EntityTypeBuilder<AdministradorPerfil> builder)
    {
        builder.ToTable("AdministradorPerfil");

        builder.HasKey(ap => ap.UsuarioId);

        builder.Property(ap => ap.UsuarioId)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("usuario_id");

        builder.Property(ap => ap.Rol)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("rol");
    }
}
