namespace RedSocial.Domain.Entities;

// Entidad pura: no conoce EF, BCrypt ni nada externo.
// Nunca guarda la contraseña en texto plano, solo su hash.
public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoLogin { get; set; }
}
