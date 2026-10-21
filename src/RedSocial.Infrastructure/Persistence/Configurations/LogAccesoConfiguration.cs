using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class LogAccesoConfiguration : IEntityTypeConfiguration<LogAcceso>
{
    public void Configure(EntityTypeBuilder<LogAcceso> builder)
    {
        builder.ToTable("LogsAcceso");
        builder.HasKey(x => x.IdLogAcceso);
        builder.Property(x => x.Identificador).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Evento).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DireccionIp).HasMaxLength(45).IsUnicode(false);
        builder.HasIndex(x => new { x.FechaUtc, x.IdLogAcceso });
        builder.HasIndex(x => x.IdUsuario);
        // Sin cascada: conservar el historial aunque se elimine una cuenta.
    }
}
