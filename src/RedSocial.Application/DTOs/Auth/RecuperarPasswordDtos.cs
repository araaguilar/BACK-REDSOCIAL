using System.ComponentModel.DataAnnotations;

namespace RedSocial.Application.DTOs.Auth;

public class SolicitarRecuperacionPasswordRequestDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;
}

public class VerificarRecuperacionPasswordRequestDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe tener 6 dígitos")]
    public string Codigo { get; set; } = string.Empty;
}

public class VerificarRecuperacionPasswordResponseDto
{
    public string RecoveryToken { get; set; } = string.Empty;
}

public class CambiarPasswordRequestDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(64, MinimumLength = 64)]
    public string RecoveryToken { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(72)]
    [RegularExpression(@"^(?=.*[A-ZÁÉÍÓÚÑ])(?=.*[^A-Za-z0-9ÁÉÍÓÚáéíóúÑñ]).{8,}$", ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula y un carácter especial")]
    public string NuevaPassword { get; set; } = string.Empty;
}
