using ElohimShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElohimShop.Infrastructure.Persistence.Configurations;

public class TokenRevocadoConfiguration : IEntityTypeConfiguration<TokenRevocado>
{
    public void Configure(EntityTypeBuilder<TokenRevocado> builder)
    {
        builder.ToTable("TokenRevocado");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(token => token.Jti)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("jti");

        builder.Property(token => token.ClienteId)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("cliente_id");

        builder.Property(token => token.ExpiraEn)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("expira_en");

        builder.Property(token => token.RevocadoEn)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("revocado_en");

        builder.HasOne(token => token.Cliente)
            .WithMany()
            .HasForeignKey(token => token.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(token => token.Jti)
            .IsUnique();
    }
}