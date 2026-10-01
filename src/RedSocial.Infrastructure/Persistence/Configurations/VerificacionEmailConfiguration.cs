using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class VerificacionEmailConfiguration : IEntityTypeConfiguration<VerificacionEmail>
{
    public void Configure(EntityTypeBuilder<VerificacionEmail> builder)
    {
        builder.ToTable("VerificacionesEmail");
        builder.HasKey(v => v.IdVerificacionEmail);

        builder.Property(v => v.Email).HasMaxLength(100).IsRequired();
        builder.Property(v => v.CodigoHash).HasMaxLength(64).IsUnicode(false).IsRequired();
        builder.Property(v => v.TokenVerificacion).HasMaxLength(64).IsUnicode(false).IsRequired();
        builder.Property(v => v.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(v => v.Email);
        builder.HasIndex(v => v.TokenVerificacion).IsUnique();
    }
}
