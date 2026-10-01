namespace RedSocial.Application.DTOs.Auth;

public class DisponibilidadEmailResponseDto
{
    public string Email { get; set; } = string.Empty;
    public bool Disponible { get; set; }
}
