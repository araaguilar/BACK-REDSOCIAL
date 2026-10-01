using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class HistorialCambioPerfilConfiguration : IEntityTypeConfiguration<HistorialCambioPerfil>
{
    public void Configure(EntityTypeBuilder<HistorialCambioPerfil> builder)
    {
        builder.ToTable("HistorialCambiosPerfil");
        builder.HasKey(h => h.IdCambio);

        builder.Property(h => h.TipoCambio).HasMaxLength(30).IsRequired();
        builder.Property(h => h.ValorAnterior).HasMaxLength(100);
        builder.Property(h => h.ValorNuevo).HasMaxLength(100);
        builder.Property(h => h.FechaCambio).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(h => new { h.IdUsuario, h.TipoCambio, h.FechaCambio });

        builder.HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
