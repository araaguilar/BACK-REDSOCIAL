using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("UsuarioRoles");
        builder.HasKey(ur => new { ur.IdUsuario, ur.IdRol });

        builder.Property(ur => ur.FechaAsignacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(ur => ur.Usuario)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(ur => ur.IdRol)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
