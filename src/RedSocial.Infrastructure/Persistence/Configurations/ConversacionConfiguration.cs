using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class ConversacionConfiguration : IEntityTypeConfiguration<Conversacion>
{
    public void Configure(EntityTypeBuilder<Conversacion> builder)
    {
        builder.ToTable("Conversaciones");
        builder.HasKey(c => c.IdConversacion);
        builder.Property(c => c.HashConversacion).HasMaxLength(128).IsRequired();
        builder.Property(c => c.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(c => c.FechaActualizacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(c => c.HashConversacion).IsUnique();
        builder.HasIndex(c => new { c.IdUsuarioA, c.IdUsuarioB }).IsUnique();
    }
}
