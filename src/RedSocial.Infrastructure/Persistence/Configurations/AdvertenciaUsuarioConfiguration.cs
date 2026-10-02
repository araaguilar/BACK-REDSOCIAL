using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class AdvertenciaUsuarioConfiguration : IEntityTypeConfiguration<AdvertenciaUsuario>
{
    public void Configure(EntityTypeBuilder<AdvertenciaUsuario> builder)
    {
        builder.ToTable("AdvertenciasUsuario");
        builder.HasKey(a => a.IdAdvertencia);
        builder.Property(a => a.Plantilla).HasMaxLength(60).IsRequired();
        builder.Property(a => a.Mensaje).HasMaxLength(700).IsRequired();
        builder.Property(a => a.Leida).HasDefaultValue(false);
        builder.Property(a => a.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(a => new { a.IdUsuario, a.Leida, a.FechaCreacion });
    }
}
