using System.ComponentModel.DataAnnotations;

namespace RedSocial.Application.DTOs.Auth;

public class RegistroRequestDto
{
    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Solo letras, números, punto y guion bajo")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    // Por ahora sin política de complejidad; solo el tope de 72 bytes que admite BCrypt.
    [Required, MaxLength(72)]
    public string Password { get; set; } = string.Empty;
}
