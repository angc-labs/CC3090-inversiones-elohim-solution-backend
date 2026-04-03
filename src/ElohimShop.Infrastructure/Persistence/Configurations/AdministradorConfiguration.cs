using ElohimShop.Domain.Entities;
using ElohimShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class AdministradorConfiguration : IEntityTypeConfiguration<Administrador>
{
    public void Configure(EntityTypeBuilder<Administrador> builder)
    {
        builder.ToTable("Administrador");

        builder.HasKey(a => a.IdUsuario);

        builder.Property(a => a.IdUsuario)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_usuario");

        builder.Property(a => a.Correo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(a => a.Apellido)
            .HasMaxLength(30);

        builder.Property(a => a.Telefono)
            .HasMaxLength(30);

        builder.Property(a => a.Contrasena)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.IdRol)
            .HasMaxLength(255)
            .HasColumnName("id_rol");

        builder.Property(a => a.Estado)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v => string.IsNullOrEmpty(v) ? null : (EstadoAdministrador)Enum.Parse(typeof(EstadoAdministrador), v))
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(a => a.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_creacion");

        builder.HasOne(a => a.Rol)
            .WithMany(r => r.Administradores)
            .HasForeignKey(a => a.IdRol)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Consultas)
            .WithOne(co => co.Administrador)
            .HasForeignKey(co => co.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Ventas)
            .WithOne(v => v.UsuarioCajero)
            .HasForeignKey(v => v.UsuarioCajeroId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.Correo)
            .IsUnique();
    }
}
