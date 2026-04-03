using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class ConsultaConfiguration : IEntityTypeConfiguration<Consulta>
{
    public void Configure(EntityTypeBuilder<Consulta> builder)
    {
        builder.ToTable("Consulta");

        builder.HasKey(co => co.IdConsulta);

        builder.Property(co => co.IdConsulta)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_consulta");

        builder.Property(co => co.IdCliente)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_cliente");

        builder.Property(co => co.IdUsuario)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_usuario");

        builder.Property(co => co.FechaConsulta)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("fecha_consulta");

        builder.HasOne(co => co.Cliente)
            .WithMany(c => c.Consultas)
            .HasForeignKey(co => co.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(co => co.Administrador)
            .WithMany(a => a.Consultas)
            .HasForeignKey(co => co.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
