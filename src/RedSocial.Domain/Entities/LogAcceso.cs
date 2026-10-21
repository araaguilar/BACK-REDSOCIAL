namespace RedSocial.Domain.Entities;

public class LogAcceso
{
    public long IdLogAcceso { get; set; }
    public int? IdUsuario { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
    public bool Exitoso { get; set; }
    public DateTimeOffset FechaUtc { get; set; }
    public string? DireccionIp { get; set; }
}
