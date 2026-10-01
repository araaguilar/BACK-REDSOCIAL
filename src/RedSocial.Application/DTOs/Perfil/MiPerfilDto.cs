namespace RedSocial.Application.DTOs.Perfil;

public class MiPerfilDto
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombrePerfil { get; set; } = string.Empty;
    public string? SobreMi { get; set; }
    public int Seguidores { get; set; }
    public int Seguidos { get; set; }
    public int TotalMeEncanta { get; set; }
}
