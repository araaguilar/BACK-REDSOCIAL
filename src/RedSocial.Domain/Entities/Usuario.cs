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
    public string EstadoCuenta { get; set; } = "activa";
    public DateTime? FechaDesactivacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    public string? MotivoEstadoCuenta { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoLogin { get; set; }
    public PerfilUsuario? Perfil { get; set; }
    public ICollection<UsuarioRol> Roles { get; set; } = [];
}
