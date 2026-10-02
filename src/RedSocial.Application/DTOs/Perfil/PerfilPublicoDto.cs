namespace RedSocial.Application.DTOs.Perfil;

public class PerfilPublicoDto
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombrePerfil { get; set; } = string.Empty;
    public string? SobreMi { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public int Seguidores { get; set; }
    public int Seguidos { get; set; }
    public int TotalMeEncanta { get; set; }
    public bool Siguiendo { get; set; }
}
