using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class SeguidorConfiguration : IEntityTypeConfiguration<Seguidor>
{
    public void Configure(EntityTypeBuilder<Seguidor> builder)
    {
        builder.ToTable("Seguidores");
        builder.HasKey(s => s.IdSeguidorRelacion);
        builder.Property(s => s.FechaSeguimiento).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(s => new { s.IdSeguidor, s.IdSeguido }).IsUnique();
        builder.HasIndex(s => s.IdSeguidor);
        builder.HasIndex(s => s.IdSeguido);
    }
}
