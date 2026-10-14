using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class ComentarioMomentoConfiguration : IEntityTypeConfiguration<ComentarioMomento>
{
    public void Configure(EntityTypeBuilder<ComentarioMomento> builder)
    {
        builder.ToTable("ComentariosMomentos");
        builder.HasKey(c => c.IdComentario);

        builder.Property(c => c.Texto).HasMaxLength(300).IsRequired();
        builder.Property(c => c.TotalMeGusta).HasDefaultValue(0);
        builder.Property(c => c.TotalRespuestas).HasDefaultValue(0);
        builder.Property(c => c.Activo).HasDefaultValue(true);
        builder.Property(c => c.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(c => new { c.IdMomento, c.IdComentarioPadre, c.Activo, c.FechaCreacion });
        builder.HasIndex(c => new { c.IdUsuario, c.FechaCreacion });

        builder.HasOne(c => c.Momento)
            .WithMany(m => m.Comentarios)
            .HasForeignKey(c => c.IdMomento)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.ComentarioPadre)
            .WithMany(c => c.Respuestas)
            .HasForeignKey(c => c.IdComentarioPadre)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
