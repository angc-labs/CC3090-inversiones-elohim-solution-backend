using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class ClientePerfilConfiguration : IEntityTypeConfiguration<ClientePerfil>
{
    public void Configure(EntityTypeBuilder<ClientePerfil> builder)
    {
        builder.ToTable("ClientePerfil");

        builder.HasKey(cp => cp.UsuarioId);

        builder.Property(cp => cp.UsuarioId)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("usuario_id");

        builder.Property(cp => cp.Direccion)
            .HasColumnType("text")
            .HasColumnName("direccion");

        builder.Property(cp => cp.TipoCliente)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("tipo_cliente");
    }
}
