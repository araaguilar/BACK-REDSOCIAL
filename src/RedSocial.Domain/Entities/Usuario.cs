namespace RedSocial.Domain.Entities;

// Entidad pura: no conoce EF, BCrypt ni nada externo.
// Nunca guarda la contraseña en texto plano, solo su hash.
public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombrePerfil { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateOnly? FechaNacimiento { get; set; }
    public string Rol { get; set; } = "usuario";
    public bool EmailVerificado { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoLogin { get; set; }
    public PerfilUsuario? Perfil { get; set; }
}
