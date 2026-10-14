using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence.Configurations;

public class MensajeChatConfiguration : IEntityTypeConfiguration<MensajeChat>
{
    public void Configure(EntityTypeBuilder<MensajeChat> builder)
    {
        builder.ToTable("MensajesChat");
        builder.HasKey(m => m.IdMensaje);
        builder.Property(m => m.Texto).HasMaxLength(1000).IsRequired();
        builder.Property(m => m.FechaEnvio).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(m => m.Leido).HasDefaultValue(false);
        builder.HasIndex(m => new { m.IdConversacion, m.FechaEnvio });

        builder.HasOne(m => m.Conversacion)
            .WithMany(c => c.Mensajes)
            .HasForeignKey(m => m.IdConversacion)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
