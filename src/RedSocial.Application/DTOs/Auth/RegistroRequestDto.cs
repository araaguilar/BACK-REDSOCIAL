using System.ComponentModel.DataAnnotations;

namespace RedSocial.Application.DTOs.Auth;

public class RegistroRequestDto
{
    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Solo letras, números, punto y guion bajo")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 2)]
    public string NombrePerfil { get; set; } = string.Empty;

    [Required]
    public DateOnly? FechaNacimiento { get; set; }

    // Tope de 72 bytes que admite BCrypt.
    [Required, MinLength(8), MaxLength(72)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(64, MinimumLength = 64)]
    public string VerificationToken { get; set; } = string.Empty;
}
