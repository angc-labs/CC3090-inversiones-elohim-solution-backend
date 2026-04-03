using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Cliente");

        builder.HasKey(c => c.IdCliente);

        builder.Property(c => c.IdCliente)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_cliente");

        builder.Property(c => c.Correo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.Apellido)
            .HasMaxLength(30);

        builder.Property(c => c.Telefono)
            .HasMaxLength(30);

        builder.Property(c => c.Contrasena)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Direccion)
            .HasColumnType("text");

        builder.Property(c => c.TipoClienteId)
            .HasMaxLength(255)
            .HasColumnName("tipo_cliente_id");

        builder.Property(c => c.FechaRegistro)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(c => c.EstadoCuenta)
            .IsRequired()
            .HasColumnName("estado_cuenta");

        builder.HasOne(c => c.TipoCliente)
            .WithMany(tc => tc.Clientes)
            .HasForeignKey(c => c.TipoClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Consultas)
            .WithOne(co => co.Cliente)
            .HasForeignKey(co => co.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Reservaciones)
            .WithOne(r => r.Cliente)
            .HasForeignKey(r => r.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.Correo)
            .IsUnique();
    }
}
