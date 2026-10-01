using System.ComponentModel.DataAnnotations;

namespace RedSocial.Application.DTOs.Auth;

public class RegistroRequestDto
{
    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^(?=.*_)[a-zA-Z0-9_.]+$", ErrorMessage = "El usuario debe incluir al menos un guion bajo")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 2)]
    public string NombrePerfil { get; set; } = string.Empty;

    [Required]
    public DateOnly? FechaNacimiento { get; set; }

    // Tope de 72 bytes que admite BCrypt.
    [Required, MinLength(8), MaxLength(72)]
    [RegularExpression(@"^(?=.*[A-ZÁÉÍÓÚÑ])(?=.*[^A-Za-z0-9ÁÉÍÓÚáéíóúÑñ]).{8,}$", ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula y un carácter especial")]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(64, MinimumLength = 64)]
    public string VerificationToken { get; set; } = string.Empty;
}
