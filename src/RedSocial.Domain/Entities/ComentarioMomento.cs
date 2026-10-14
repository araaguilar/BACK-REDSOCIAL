namespace RedSocial.Domain.Entities;

public class ComentarioMomento
{
    public int IdComentario { get; set; }
    public int IdMomento { get; set; }
    public int IdUsuario { get; set; }
    public int? IdComentarioPadre { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int TotalMeGusta { get; set; }
    public int TotalRespuestas { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEliminacion { get; set; }

    public Momento? Momento { get; set; }
    public Usuario? Usuario { get; set; }
    public ComentarioMomento? ComentarioPadre { get; set; }
    public ICollection<ComentarioMomento> Respuestas { get; set; } = [];
    public ICollection<ComentarioMomentoMeGusta> MeGusta { get; set; } = [];
}
