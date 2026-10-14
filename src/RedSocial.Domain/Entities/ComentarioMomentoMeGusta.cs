namespace RedSocial.Domain.Entities;

public class ComentarioMomentoMeGusta
{
    public int IdComentarioMeGusta { get; set; }
    public int IdComentario { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ComentarioMomento? Comentario { get; set; }
    public Usuario? Usuario { get; set; }
}
