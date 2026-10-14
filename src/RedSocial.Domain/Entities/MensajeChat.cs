namespace RedSocial.Domain.Entities;

public class MensajeChat
{
    public int IdMensaje { get; set; }
    public int IdConversacion { get; set; }
    public int IdEmisor { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    public bool Leido { get; set; }

    public Conversacion? Conversacion { get; set; }
}
