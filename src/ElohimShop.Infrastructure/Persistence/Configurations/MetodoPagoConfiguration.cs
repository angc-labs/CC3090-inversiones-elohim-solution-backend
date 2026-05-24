using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("MetodoPago");

        builder.HasKey(mp => mp.IdMetodoPago);

        builder.Property(mp => mp.IdMetodoPago)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("id_metodo_pago");

        builder.Property(mp => mp.UsuarioId)
            .HasMaxLength(255)
            .HasColumnName("usuario_id");

        builder.Property(mp => mp.NombreMetodo)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnName("nombre_metodo");

        builder.Property(mp => mp.Descripcion)
            .HasColumnType("text")
            .HasColumnName("descripcion");

        builder.Property(mp => mp.StripePaymentMethodId)
            .HasMaxLength(255)
            .HasColumnName("stripe_payment_method_id");

        builder.Property(mp => mp.Alias)
            .HasMaxLength(120)
            .HasColumnName("alias_tarjeta");

        builder.Property(mp => mp.MarcaTarjeta)
            .HasMaxLength(30)
            .HasColumnName("marca_tarjeta");

        builder.Property(mp => mp.UltimosDigitos)
            .HasMaxLength(4)
            .HasColumnName("ultimos_digitos");

        builder.Property(mp => mp.ExpiraMes)
            .HasColumnName("expira_mes");

        builder.Property(mp => mp.ExpiraAnio)
            .HasColumnName("expira_anio");

        builder.Property(mp => mp.Activo)
            .IsRequired()
            .HasColumnName("activo");

        builder.HasMany(mp => mp.Reservaciones)
            .WithOne(r => r.MetodoPago)
            .HasForeignKey(r => r.MetodoPagoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
