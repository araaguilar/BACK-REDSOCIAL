using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class PerfilUsuarioConfiguration : IEntityTypeConfiguration<PerfilUsuario>
{
    public void Configure(EntityTypeBuilder<PerfilUsuario> builder)
    {
        builder.ToTable("PerfilesUsuario");
        builder.HasKey(p => p.IdPerfil);

        builder.Property(p => p.NombrePerfil).HasMaxLength(60).IsRequired();
        builder.Property(p => p.Biografia).HasMaxLength(160);
        builder.Property(p => p.SobreMi).HasMaxLength(300);
        builder.Property(p => p.FotoPerfilUrl).HasMaxLength(500);
        builder.Property(p => p.TotalMeEncanta).HasDefaultValue(0);
        builder.Property(p => p.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(p => p.IdUsuario).IsUnique();
        builder.HasIndex(p => p.NombrePerfil);

        builder.HasOne(p => p.Usuario)
            .WithOne(u => u.Perfil)
            .HasForeignKey<PerfilUsuario>(p => p.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
