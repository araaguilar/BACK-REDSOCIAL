namespace RedSocial.Application.DTOs.Busqueda;

public class PerfilBusquedaDto
{
    public int IdUsuario { get; set; }
    public string NombrePerfil { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string? SobreMi { get; set; }
    public string? FotoPerfilUrl { get; set; }
}
