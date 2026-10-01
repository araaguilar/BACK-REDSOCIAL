namespace RedSocial.Application.DTOs.Auth;

// El registro no devuelve token: la sesión solo se obtiene con el login.
public class RegistroResponseDto
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombrePerfil { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
