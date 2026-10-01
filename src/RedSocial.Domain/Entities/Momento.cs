namespace RedSocial.Domain.Entities;

public class Momento
{
    public int IdMomento { get; set; }
    public int IdUsuario { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? TipoAdjunto { get; set; }
    public string? ArchivoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public int TotalMeGusta { get; set; }
    public int TotalComentarios { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    public Usuario? Usuario { get; set; }
}
