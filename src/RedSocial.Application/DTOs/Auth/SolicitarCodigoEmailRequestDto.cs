using System.ComponentModel.DataAnnotations;

namespace RedSocial.Application.DTOs.Auth;

public class SolicitarCodigoEmailRequestDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;
}
