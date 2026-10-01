using System.ComponentModel.DataAnnotations;

namespace RedSocial.Application.DTOs.Auth;

public class VerificarCodigoEmailRequestDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe tener 6 dígitos")]
    public string Codigo { get; set; } = string.Empty;
}
