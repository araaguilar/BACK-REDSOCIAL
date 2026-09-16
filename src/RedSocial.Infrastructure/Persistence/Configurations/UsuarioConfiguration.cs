using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

// El mapeo a la BD vive aquí y no en la entidad, para que Domain siga limpio.
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.IdUsuario);

        builder.Property(u => u.NombreUsuario).HasMaxLength(30).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(100).IsRequired();
        // Un hash BCrypt mide 60 caracteres ($2a$12$ + salt + hash).
        builder.Property(u => u.PasswordHash).HasMaxLength(60).IsUnicode(false).IsRequired();
        builder.Property(u => u.FechaRegistro).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(u => u.NombreUsuario).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
