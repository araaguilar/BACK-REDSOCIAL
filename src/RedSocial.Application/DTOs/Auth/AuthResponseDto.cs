namespace RedSocial.Application.DTOs.Auth;

public class AuthResponseDto
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombrePerfil { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool EsFundador { get; set; }
    public bool EmailVerificado { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
}
