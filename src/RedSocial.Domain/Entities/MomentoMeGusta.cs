namespace RedSocial.Domain.Entities;

public class MomentoMeGusta
{
    public int IdMeGusta { get; set; }
    public int IdMomento { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Momento? Momento { get; set; }
    public Usuario? Usuario { get; set; }
}
