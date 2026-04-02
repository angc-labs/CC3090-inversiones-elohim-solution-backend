using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class TipoClienteConfiguration : IEntityTypeConfiguration<TipoCliente>
{
    public void Configure(EntityTypeBuilder<TipoCliente> builder)
    {
        builder.ToTable("TipoCliente");

        builder.HasKey(tc => tc.IdTipo);

        builder.Property(tc => tc.IdTipo)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_tipo");

        builder.Property(tc => tc.Nombre)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(tc => tc.Descripcion)
            .HasColumnType("text");

        builder.Property(tc => tc.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp");

        builder.HasMany(tc => tc.Clientes)
            .WithOne(c => c.TipoCliente)
            .HasForeignKey(c => c.TipoClienteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
