namespace RedSocial.Application.DTOs.Auth;

public class DisponibilidadUsuarioResponseDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public bool Disponible { get; set; }
}
