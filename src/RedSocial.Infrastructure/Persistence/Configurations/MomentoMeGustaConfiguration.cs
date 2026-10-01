using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class MomentoMeGustaConfiguration : IEntityTypeConfiguration<MomentoMeGusta>
{
    public void Configure(EntityTypeBuilder<MomentoMeGusta> builder)
    {
        builder.ToTable("MeGustaMomentos");
        builder.HasKey(m => m.IdMeGusta);

        builder.Property(m => m.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(m => new { m.IdMomento, m.IdUsuario }).IsUnique();
        builder.HasIndex(m => new { m.IdUsuario, m.FechaCreacion });

        builder.HasOne(m => m.Momento)
            .WithMany()
            .HasForeignKey(m => m.IdMomento)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
