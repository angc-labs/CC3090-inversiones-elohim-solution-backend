using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id");

        builder.Property(u => u.Correo)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("correo");

        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(30)
            .HasColumnName("nombre");

        builder.Property(u => u.Apellido)
            .HasMaxLength(30)
            .HasColumnName("apellido");

        builder.Property(u => u.Telefono)
            .HasMaxLength(30)
            .HasColumnName("telefono");

        builder.Property(u => u.Contrasena)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("contrasena");

        builder.Property(u => u.TipoUsuario)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("tipo_usuario");

        builder.Property(u => u.Estado)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("estado");

        builder.Property(u => u.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_creacion");

        builder.HasOne(u => u.ClientePerfil)
            .WithOne(cp => cp.Usuario)
            .HasForeignKey<ClientePerfil>(cp => cp.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.AdministradorPerfil)
            .WithOne(ap => ap.Usuario)
            .HasForeignKey<AdministradorPerfil>(ap => ap.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ConsultasCliente)
            .WithOne(c => c.Cliente)
            .HasForeignKey(c => c.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ConsultasAdministrador)
            .WithOne(c => c.Administrador)
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Reservaciones)
            .WithOne(r => r.Cliente)
            .HasForeignKey(r => r.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.Ventas)
            .WithOne(v => v.UsuarioCajero)
            .HasForeignKey(v => v.UsuarioCajeroId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => u.Correo)
            .IsUnique();
    }
}
