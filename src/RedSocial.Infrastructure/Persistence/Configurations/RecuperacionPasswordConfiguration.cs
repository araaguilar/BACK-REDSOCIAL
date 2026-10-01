using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class RecuperacionPasswordConfiguration : IEntityTypeConfiguration<RecuperacionPassword>
{
    public void Configure(EntityTypeBuilder<RecuperacionPassword> builder)
    {
        builder.ToTable("RecuperacionesPassword");
        builder.HasKey(r => r.IdRecuperacionPassword);

        builder.Property(r => r.Email).HasMaxLength(100).IsRequired();
        builder.Property(r => r.CodigoHash).HasMaxLength(64).IsUnicode(false).IsRequired();
        builder.Property(r => r.TokenRecuperacion).HasMaxLength(64).IsUnicode(false).IsRequired();
        builder.Property(r => r.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(r => r.Email);
        builder.HasIndex(r => r.IdUsuario);
        builder.HasIndex(r => r.TokenRecuperacion).IsUnique();

        builder.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
