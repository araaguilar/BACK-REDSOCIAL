using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.IdRol);

        builder.Property(r => r.Nombre).HasMaxLength(30).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(160).IsRequired();
        builder.Property(r => r.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(r => r.Nombre).IsUnique();
    }
}
