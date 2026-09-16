namespace RedSocial.Application.DTOs.Auth;

public class AuthResponseDto
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
}
