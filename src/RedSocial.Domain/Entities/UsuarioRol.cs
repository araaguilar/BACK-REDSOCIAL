namespace RedSocial.Domain.Entities;

public class UsuarioRol
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public int? AsignadoPor { get; set; }
    public Usuario? Usuario { get; set; }
    public Rol? Rol { get; set; }
}
