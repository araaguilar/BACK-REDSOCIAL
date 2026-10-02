using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class ModeracionAccionConfiguration : IEntityTypeConfiguration<ModeracionAccion>
{
    public void Configure(EntityTypeBuilder<ModeracionAccion> builder)
    {
        builder.ToTable("ModeracionAcciones");
        builder.HasKey(a => a.IdAccion);
        builder.Property(a => a.Tipo).HasMaxLength(30).IsRequired();
        builder.Property(a => a.Motivo).HasMaxLength(160).IsRequired();
        builder.Property(a => a.Mensaje).HasMaxLength(700);
        builder.Property(a => a.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(a => new { a.IdUsuarioObjetivo, a.FechaCreacion });
    }
}
