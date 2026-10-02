using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class MomentoConfiguration : IEntityTypeConfiguration<Momento>
{
    public void Configure(EntityTypeBuilder<Momento> builder)
    {
        builder.ToTable("Momentos");
        builder.HasKey(m => m.IdMomento);

        builder.Property(m => m.Texto).HasMaxLength(180).IsRequired();
        builder.Property(m => m.TipoAdjunto).HasMaxLength(20);
        builder.Property(m => m.ArchivoUrl).HasMaxLength(500);
        builder.Property(m => m.LinkUrl).HasMaxLength(500);
        builder.Property(m => m.MotivoEliminacion).HasMaxLength(200);
        builder.Property(m => m.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(m => m.Activo).HasDefaultValue(true);

        builder.HasIndex(m => new { m.IdUsuario, m.FechaCreacion });
        builder.HasIndex(m => new { m.Activo, m.FechaCreacion });
        builder.HasIndex(m => new { m.Activo, m.EliminarDefinitivamenteEn });

        builder.HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
