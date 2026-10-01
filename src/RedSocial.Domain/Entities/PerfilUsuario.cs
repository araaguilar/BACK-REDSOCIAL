namespace RedSocial.Domain.Entities;

public class PerfilUsuario
{
    public int IdPerfil { get; set; }
    public int IdUsuario { get; set; }
    public string NombrePerfil { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public string? Biografia { get; set; }
    public string? SobreMi { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public int TotalMeEncanta { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    public Usuario? Usuario { get; set; }
}
