using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class ReporteConfiguration : IEntityTypeConfiguration<Reporte>
{
    public void Configure(EntityTypeBuilder<Reporte> builder)
    {
        builder.ToTable("Reportes");
        builder.HasKey(r => r.IdReporte);
        builder.Property(r => r.Tipo).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Motivo).HasMaxLength(40).IsRequired();
        builder.Property(r => r.Detalle).HasMaxLength(500);
        builder.Property(r => r.Estado).HasMaxLength(20).HasDefaultValue("abierto").IsRequired();
        builder.Property(r => r.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(r => new { r.Estado, r.FechaCreacion });
        builder.HasIndex(r => r.IdUsuarioReportado);
        builder.HasIndex(r => r.IdMomentoReportado);
    }
}
