using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class ComentarioMomentoMeGustaConfiguration : IEntityTypeConfiguration<ComentarioMomentoMeGusta>
{
    public void Configure(EntityTypeBuilder<ComentarioMomentoMeGusta> builder)
    {
        builder.ToTable("MeGustaComentariosMomentos");
        builder.HasKey(m => m.IdComentarioMeGusta);

        builder.Property(m => m.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(m => new { m.IdComentario, m.IdUsuario }).IsUnique();

        builder.HasOne(m => m.Comentario)
            .WithMany(c => c.MeGusta)
            .HasForeignKey(m => m.IdComentario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
