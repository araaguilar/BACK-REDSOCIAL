namespace RedSocial.Domain.Entities;

public class ModeracionAccion
{
    public int IdAccion { get; set; }
    public int IdModerador { get; set; }
    public int IdUsuarioObjetivo { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
