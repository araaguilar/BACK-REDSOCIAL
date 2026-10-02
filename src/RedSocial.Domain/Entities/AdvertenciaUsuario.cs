namespace RedSocial.Domain.Entities;

public class AdvertenciaUsuario
{
    public int IdAdvertencia { get; set; }
    public int IdUsuario { get; set; }
    public int IdModerador { get; set; }
    public string Plantilla { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
