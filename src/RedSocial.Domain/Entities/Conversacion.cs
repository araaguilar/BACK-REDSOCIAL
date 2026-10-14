namespace RedSocial.Domain.Entities;

public class Conversacion
{
    public int IdConversacion { get; set; }
    public string HashConversacion { get; set; } = string.Empty;
    public int IdUsuarioA { get; set; }
    public int IdUsuarioB { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public ICollection<MensajeChat> Mensajes { get; set; } = [];
}
