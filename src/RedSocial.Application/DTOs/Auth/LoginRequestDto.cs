using System.ComponentModel.DataAnnotations;

namespace RedSocial.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El usuario o email es requerido")]
    [MaxLength(100)]
    public string UsuarioOEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MaxLength(72)] // BCrypt solo considera los primeros 72 bytes
    public string Password { get; set; } = string.Empty;
}
